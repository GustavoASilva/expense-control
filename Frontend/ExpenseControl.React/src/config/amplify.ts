import { Amplify } from 'aws-amplify';

const useMockAuth = import.meta.env.VITE_USE_MOCK_AUTH === 'true';
const configuredRegion = import.meta.env.VITE_COGNITO_REGION?.trim();
const userPoolId = import.meta.env.VITE_COGNITO_USER_POOL_ID?.trim();
const userPoolClientId = import.meta.env.VITE_COGNITO_APP_CLIENT_ID?.trim();

if (useMockAuth) {
  console.warn('Mock auth is enabled (VITE_USE_MOCK_AUTH=true). Cognito is disabled.');
} else if (!userPoolId || !userPoolClientId) {
  console.error('Cognito is not configured. Missing VITE_COGNITO_USER_POOL_ID or VITE_COGNITO_APP_CLIENT_ID.');
} else if (configuredRegion && !userPoolId.startsWith(`${configuredRegion}_`)) {
  console.error(`Cognito configuration mismatch. VITE_COGNITO_REGION is ${configuredRegion} but userPoolId is ${userPoolId}.`);
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
