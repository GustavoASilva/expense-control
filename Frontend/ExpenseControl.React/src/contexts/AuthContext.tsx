import React, { useEffect, useState, useCallback } from 'react';
import { getCurrentUser, signIn, signOut, fetchAuthSession, AuthUser } from 'aws-amplify/auth';
import { AuthContext } from '../hooks/useAuth';
import { getUserHousehold } from '../services/api';

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [hasHousehold, setHasHousehold] = useState<boolean | null>(null);
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

  const login = async (username: string, password: string) => {
    if (isMock) {
      setUser({ username: 'mock-user', userId: 'mock-user-id' } as AuthUser);
      await checkHousehold();
      return;
    }

    await signIn({ username, password });
    await checkUser();
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
        login,
        logout,
        getAccessToken,
        checkHousehold,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
