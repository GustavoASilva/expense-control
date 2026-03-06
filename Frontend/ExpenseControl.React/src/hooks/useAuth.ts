import { createContext, useContext } from 'react';
import { AuthUser } from 'aws-amplify/auth';

export interface AuthContextType {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  hasHousehold: boolean | null;
  householdName: string | null;
  requiresPasswordReset: boolean;
  requiresMfaSetup: boolean;
  requiresMfa: boolean;
  mfaMethod: 'TOTP' | null;
  totpSetupUri: string | null;
  totpSetupSecret: string | null;
  login: (username: string, password: string) => Promise<{ requiresPasswordReset: boolean; requiresMfa: boolean; requiresMfaSetup: boolean }>;
  completeNewPassword: (newPassword: string) => Promise<{ requiresMfa: boolean; requiresMfaSetup: boolean }>;
  completeTotpSetup: (code: string) => Promise<void>;
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
