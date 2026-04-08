import React, { useEffect, useState, useCallback } from 'react';
import { getCurrentUser, signIn, signOut, fetchAuthSession, confirmSignIn, AuthUser } from 'aws-amplify/auth';
import { AuthContext } from '../hooks/useAuth';
import { getUserHousehold } from '../services/api';

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [hasHousehold, setHasHousehold] = useState<boolean | null>(null);
  const [householdName, setHouseholdName] = useState<string | null>(null);
  const [requiresPasswordReset, setRequiresPasswordReset] = useState(false);
  const [requiresMfaSetup, setRequiresMfaSetup] = useState(false);
  const [requiresMfa, setRequiresMfa] = useState(false);
  const [mfaMethod, setMfaMethod] = useState<'TOTP' | null>(null);
  const [totpSetupUri, setTotpSetupUri] = useState<string | null>(null);
  const [totpSetupSecret, setTotpSetupSecret] = useState<string | null>(null);
  const isMock = import.meta.env.VITE_USE_MOCK_AUTH === 'true';

  const handleSignInStep = useCallback(async (nextStep?: { signInStep?: string; totpSetupDetails?: { sharedSecret?: string; getSetupUri?: (appName: string) => URL } }) => {
    const signInStep = nextStep?.signInStep;

    if (signInStep === 'CONFIRM_SIGN_IN_WITH_NEW_PASSWORD_REQUIRED') {
      setRequiresPasswordReset(true);
      setRequiresMfaSetup(false);
      setRequiresMfa(false);
      setMfaMethod(null);
      setTotpSetupUri(null);
      setTotpSetupSecret(null);
      setUser(null);
      setHasHousehold(null);
      return 'NEW_PASSWORD_REQUIRED' as const;
    }

    if (signInStep === 'CONTINUE_SIGN_IN_WITH_MFA_SETUP_SELECTION') {
      const selectionResult = await confirmSignIn({ challengeResponse: 'TOTP' });
      return handleSignInStep(selectionResult.nextStep);
    }

    if (signInStep === 'CONTINUE_SIGN_IN_WITH_TOTP_SETUP') {
      const setupUri = nextStep?.totpSetupDetails?.getSetupUri?.('ExpenseCtrl')?.toString() ?? null;
      const setupSecret = nextStep?.totpSetupDetails?.sharedSecret ?? null;

      setRequiresPasswordReset(false);
      setRequiresMfaSetup(true);
      setRequiresMfa(false);
      setMfaMethod('TOTP');
      setTotpSetupUri(setupUri);
      setTotpSetupSecret(setupSecret);
      setUser(null);
      setHasHousehold(null);
      return 'MFA_SETUP_REQUIRED' as const;
    }

    if (signInStep === 'CONFIRM_SIGN_IN_WITH_TOTP_CODE') {
      setRequiresPasswordReset(false);
      setRequiresMfaSetup(false);
      setRequiresMfa(true);
      setMfaMethod('TOTP');
      setTotpSetupUri(null);
      setTotpSetupSecret(null);
      setUser(null);
      setHasHousehold(null);
      return 'MFA_REQUIRED' as const;
    }

    setRequiresPasswordReset(false);
    setRequiresMfaSetup(false);
    setRequiresMfa(false);
    setMfaMethod(null);
    setTotpSetupUri(null);
    setTotpSetupSecret(null);
    return 'DONE' as const;
  }, []);

  const checkHousehold = useCallback(async () => {
    try {
      const response = await getUserHousehold();
      setHasHousehold(response.household !== null);
      setHouseholdName(response.household?.name ?? null);
    } catch {
      setHasHousehold(false);
      setHouseholdName(null);
    }
  }, []);

  const checkUser = useCallback(async () => {
    if (isMock) {
      setUser({ username: 'mock-user', userId: 'mock-user-id' } as AuthUser);
      await checkHousehold();
      setIsLoading(false);
      return;
    }

    try {
      const currentUser = await getCurrentUser();
      setUser(currentUser);
      setRequiresPasswordReset(false);
      setRequiresMfaSetup(false);
      setRequiresMfa(false);
      setMfaMethod(null);
      setTotpSetupUri(null);
      setTotpSetupSecret(null);
      await checkHousehold();
    } catch {
      setUser(null);
      setHasHousehold(null);
    } finally {
      setIsLoading(false);
    }
  }, [isMock, checkHousehold]);

  useEffect(() => {
    checkUser();
  }, [checkUser]);

  const login = async (username: string, password: string): Promise<{ requiresPasswordReset: boolean; requiresMfa: boolean; requiresMfaSetup: boolean }> => {
    if (isMock) {
      setUser({ username: 'mock-user', userId: 'mock-user-id' } as AuthUser);
      await checkHousehold();
      return { requiresPasswordReset: false, requiresMfa: false, requiresMfaSetup: false };
    }

    let signInResult;
    try {
      signInResult = await signIn({ username, password });
    } catch (error) {
      if (error instanceof Error && error.message === 'There is already a signed in user.') {
        await signOut();
        signInResult = await signIn({ username, password });
      } else {
        throw error;
      }
    }
    const nextAction = await handleSignInStep(signInResult.nextStep);

    if (nextAction === 'NEW_PASSWORD_REQUIRED') {
      return { requiresPasswordReset: true, requiresMfa: false, requiresMfaSetup: false };
    }

    if (nextAction === 'MFA_SETUP_REQUIRED') {
      return { requiresPasswordReset: false, requiresMfa: false, requiresMfaSetup: true };
    }

    if (nextAction === 'MFA_REQUIRED') {
      return { requiresPasswordReset: false, requiresMfa: true, requiresMfaSetup: false };
    }

    await checkUser();
    return { requiresPasswordReset: false, requiresMfa: false, requiresMfaSetup: false };
  };

  const completeNewPassword = async (newPassword: string): Promise<{ requiresMfa: boolean; requiresMfaSetup: boolean }> => {
    if (isMock) {
      setRequiresPasswordReset(false);
      return { requiresMfa: false, requiresMfaSetup: false };
    }

    const confirmResult = await confirmSignIn({ challengeResponse: newPassword });

    const nextAction = await handleSignInStep(confirmResult.nextStep);

    if (nextAction === 'MFA_SETUP_REQUIRED') {
      return { requiresMfa: false, requiresMfaSetup: true };
    }

    if (nextAction === 'MFA_REQUIRED') {
      return { requiresMfa: true, requiresMfaSetup: false };
    }

    if (nextAction === 'DONE') {
      setRequiresPasswordReset(false);
      await checkUser();
      return { requiresMfa: false, requiresMfaSetup: false };
    }

    throw new Error('Unable to complete password reset.');
  };

  const completeTotpSetup = async (code: string) => {
    if (isMock) {
      setRequiresMfaSetup(false);
      setTotpSetupUri(null);
      setTotpSetupSecret(null);
      return;
    }

    const confirmResult = await confirmSignIn({ challengeResponse: code });
    const nextAction = await handleSignInStep(confirmResult.nextStep);

    if (nextAction === 'DONE') {
      await checkUser();
      return;
    }

    throw new Error('Unable to complete TOTP setup.');
  };

  const verifyMfaCode = async (code: string) => {
    if (isMock) {
      setRequiresMfa(false);
      setMfaMethod(null);
      return;
    }

    const confirmResult = await confirmSignIn({ challengeResponse: code });
    const nextAction = await handleSignInStep(confirmResult.nextStep);

    if (nextAction === 'DONE') {
      setRequiresMfa(false);
      setMfaMethod(null);
      await checkUser();
      return;
    }

    throw new Error('Invalid verification code.');
  };

  const logout = async () => {
    if (isMock) {
      setUser(null);
      setHasHousehold(null);
      setHouseholdName(null);
      return;
    }

    await signOut();
    setUser(null);
    setHasHousehold(null);
    setHouseholdName(null);
    setRequiresPasswordReset(false);
    setRequiresMfaSetup(false);
    setRequiresMfa(false);
    setMfaMethod(null);
    setTotpSetupUri(null);
    setTotpSetupSecret(null);
  };

  const getAccessToken = async (): Promise<string | undefined> => {
    if (isMock) {
      return 'mock-access-token';
    }

    try {
      const session = await fetchAuthSession();
      return session.tokens?.accessToken?.toString();
    } catch {
      return undefined;
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        hasHousehold,
        householdName,
        requiresPasswordReset,
        requiresMfaSetup,
        requiresMfa,
        mfaMethod,
        totpSetupUri,
        totpSetupSecret,
        login,
        completeNewPassword,
        completeTotpSetup,
        verifyMfaCode,
        logout,
        getAccessToken,
        checkHousehold,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
