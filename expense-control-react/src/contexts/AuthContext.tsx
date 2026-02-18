import React, { useEffect, useState, useCallback } from 'react';
import { getCurrentUser, signIn, signOut, fetchAuthSession, AuthUser } from 'aws-amplify/auth';
import { AuthContext } from '../hooks/useAuth';

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const isMock = import.meta.env.VITE_USE_MOCK_AUTH === 'true';

  const checkUser = useCallback(async () => {
    if (isMock) {
      setUser({ username: 'mock-user', userId: 'mock-user-id' } as AuthUser);
      setIsLoading(false);
      return;
    }

    try {
      const currentUser = await getCurrentUser();
      setUser(currentUser);
    } catch {
      setUser(null);
    } finally {
      setIsLoading(false);
    }
  }, [isMock]);

  useEffect(() => {
    checkUser();
  }, [checkUser]);

  const login = async (username: string, password: string) => {
    if (isMock) {
      setUser({ username: 'mock-user', userId: 'mock-user-id' } as AuthUser);
      return;
    }

    await signIn({ username, password });
    await checkUser();
  };

  const logout = async () => {
    if (isMock) {
      setUser(null);
      return;
    }

    await signOut();
    setUser(null);
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
        login,
        logout,
        getAccessToken,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
