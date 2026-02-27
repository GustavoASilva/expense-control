import React, { useEffect, useState, useCallback } from 'react';
import { getCurrentUser, signIn, signOut, fetchAuthSession, confirmSignIn, AuthUser } from 'aws-amplify/auth';
import { AuthContext } from '../hooks/useAuth';
import { getUserHousehold } from '../services/api';

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [hasHousehold, setHasHousehold] = useState<boolean | null>(null);
  const [requiresPasswordReset, setRequiresPasswordReset] = useState(false);
  const isMock = import.meta.env.VITE_USE_MOCK_AUTH === 'true';

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

  const login = async (username: string, password: string): Promise<{ requiresPasswordReset: boolean }> => {
    if (isMock) {
      setUser({ username: 'mock-user', userId: 'mock-user-id' } as AuthUser);
      await checkHousehold();
      return { requiresPasswordReset: false };
    }

    const signInResult = await signIn({ username, password });

    if (signInResult.nextStep.signInStep === 'CONFIRM_SIGN_IN_WITH_NEW_PASSWORD_REQUIRED') {
      setRequiresPasswordReset(true);
      setUser(null);
      setHasHousehold(null);
      return { requiresPasswordReset: true };
    }

    setRequiresPasswordReset(false);
    await checkUser();
    return { requiresPasswordReset: false };
  };

  const completeNewPassword = async (newPassword: string) => {
    if (isMock) {
      setRequiresPasswordReset(false);
      return;
    }

    const confirmResult = await confirmSignIn({ challengeResponse: newPassword });

    if (confirmResult.nextStep.signInStep === 'DONE') {
      setRequiresPasswordReset(false);
      await checkUser();
      return;
    }

    throw new Error('Unable to complete password reset.');
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
        login,
        completeNewPassword,
        logout,
        getAccessToken,
        checkHousehold,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
