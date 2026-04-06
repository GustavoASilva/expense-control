import { Amplify } from 'aws-amplify';

const useMockAuth = import.meta.env.VITE_USE_MOCK_AUTH === 'true';
const userPoolId = import.meta.env.VITE_COGNITO_USER_POOL_ID?.trim();
const userPoolClientId = import.meta.env.VITE_COGNITO_APP_CLIENT_ID?.trim();

if (useMockAuth) {
  console.warn('Mock auth is enabled (VITE_USE_MOCK_AUTH=true). Cognito is disabled.');
} else if (!userPoolId || !userPoolClientId) {
  console.error('Cognito is not configured. Missing VITE_COGNITO_USER_POOL_ID or VITE_COGNITO_APP_CLIENT_ID.');
} else {
  Amplify.configure({
    Auth: {
      Cognito: {
        userPoolId,
        userPoolClientId,
      },
    },
  });
}
