import { Amplify } from 'aws-amplify';

if (import.meta.env.VITE_USE_MOCK_AUTH !== 'true') {
  Amplify.configure({
    Auth: {
      Cognito: {
        userPoolId: import.meta.env.VITE_COGNITO_USER_POOL_ID,
        userPoolClientId: import.meta.env.VITE_COGNITO_APP_CLIENT_ID,
      },
    },
  });
}
