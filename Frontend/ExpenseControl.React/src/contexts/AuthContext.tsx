import React, { useEffect, useState, useCallback } from 'react';
import { getCurrentUser, signIn, signOut, fetchAuthSession, confirmSignIn, AuthUser } from 'aws-amplify/auth';
import { AuthContext } from '../hooks/useAuth';
import { getUserHousehold } from '../services/api';

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [hasHousehold, setHasHousehold] = useState<boolean | null>(null);
  const [requiresPasswordReset, setRequiresPasswordReset] = useState(false);
  const [requiresMfa, setRequiresMfa] = useState(false);
  const [mfaMethod, setMfaMethod] = useState<'SMS' | 'TOTP' | null>(null);
  const isMock = import.meta.env.VITE_USE_MOCK_AUTH === 'true';

  const applySignInStep = useCallback((signInStep?: string) => {
    if (signInStep === 'CONFIRM_SIGN_IN_WITH_NEW_PASSWORD_REQUIRED') {
      setRequiresPasswordReset(true);
      setRequiresMfa(false);
      setMfaMethod(null);
      setUser(null);
      setHasHousehold(null);
      return 'NEW_PASSWORD_REQUIRED' as const;
    }

    if (signInStep === 'CONFIRM_SIGN_IN_WITH_SMS_CODE') {
      setRequiresPasswordReset(false);
      setRequiresMfa(true);
      setMfaMethod('SMS');
      setUser(null);
      setHasHousehold(null);
      return 'MFA_REQUIRED' as const;
    }

    if (signInStep === 'CONFIRM_SIGN_IN_WITH_TOTP_CODE') {
      setRequiresPasswordReset(false);
      setRequiresMfa(true);
      setMfaMethod('TOTP');
      setUser(null);
      setHasHousehold(null);
      return 'MFA_REQUIRED' as const;
    }

    setRequiresPasswordReset(false);
    setRequiresMfa(false);
    setMfaMethod(null);
    return 'DONE' as const;
  }, []);

  const checkHousehold = useCallback(async () => {
    try {
      const response = await getUserHousehold();
      setHasHousehold(response.household !== null);
    } catch {
      setHasHousehold(false);
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
      setRequiresMfa(false);
      setMfaMethod(null);
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

  const login = async (username: string, password: string): Promise<{ requiresPasswordReset: boolean; requiresMfa: boolean }> => {
    if (isMock) {
      setUser({ username: 'mock-user', userId: 'mock-user-id' } as AuthUser);
      await checkHousehold();
      return { requiresPasswordReset: false, requiresMfa: false };
    }

    const signInResult = await signIn({ username, password });
    const nextAction = applySignInStep(signInResult.nextStep.signInStep);

    if (nextAction === 'NEW_PASSWORD_REQUIRED') {
      return { requiresPasswordReset: true, requiresMfa: false };
    }

    if (nextAction === 'MFA_REQUIRED') {
      return { requiresPasswordReset: false, requiresMfa: true };
    }

    await checkUser();
    return { requiresPasswordReset: false, requiresMfa: false };
  };

  const completeNewPassword = async (newPassword: string): Promise<{ requiresMfa: boolean }> => {
    if (isMock) {
      setRequiresPasswordReset(false);
      return { requiresMfa: false };
    }

    const confirmResult = await confirmSignIn({ challengeResponse: newPassword });

    const nextAction = applySignInStep(confirmResult.nextStep.signInStep);

    if (nextAction === 'MFA_REQUIRED') {
      return { requiresMfa: true };
    }

    if (nextAction === 'DONE') {
      setRequiresPasswordReset(false);
      await checkUser();
      return { requiresMfa: false };
    }

    throw new Error('Unable to complete password reset.');
  };

  const verifyMfaCode = async (code: string) => {
    if (isMock) {
      setRequiresMfa(false);
      setMfaMethod(null);
      return;
    }

    const confirmResult = await confirmSignIn({ challengeResponse: code });
    const nextAction = applySignInStep(confirmResult.nextStep.signInStep);

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
      return;
    }

    await signOut();
    setUser(null);
    setHasHousehold(null);
    setRequiresPasswordReset(false);
    setRequiresMfa(false);
    setMfaMethod(null);
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
        requiresPasswordReset,
        requiresMfa,
        mfaMethod,
        login,
        completeNewPassword,
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
