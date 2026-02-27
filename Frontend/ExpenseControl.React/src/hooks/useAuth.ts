import { createContext, useContext } from 'react';
import { AuthUser } from 'aws-amplify/auth';

export interface AuthContextType {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  hasHousehold: boolean | null;
  requiresPasswordReset: boolean;
  requiresMfa: boolean;
  mfaMethod: 'SMS' | 'TOTP' | null;
  login: (username: string, password: string) => Promise<{ requiresPasswordReset: boolean; requiresMfa: boolean }>;
  completeNewPassword: (newPassword: string) => Promise<{ requiresMfa: boolean }>;
  verifyMfaCode: (code: string) => Promise<void>;
  logout: () => Promise<void>;
  getAccessToken: () => Promise<string | undefined>;
  checkHousehold: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
