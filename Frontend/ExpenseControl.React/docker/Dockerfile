# Build stage
FROM node:20-alpine AS build
WORKDIR /app
COPY Frontend/ExpenseControl.React/package*.json ./
RUN npm ci
COPY Frontend/ExpenseControl.React/. .
ARG VITE_USE_MOCK_AUTH=false
ARG VITE_API_URL
ARG VITE_COGNITO_USER_POOL_ID
ARG VITE_COGNITO_APP_CLIENT_ID
RUN set -eu; \
	: > .env.production.local; \
	printf 'VITE_USE_MOCK_AUTH=%s\n' "$VITE_USE_MOCK_AUTH" >> .env.production.local; \
	if [ -n "${VITE_API_URL:-}" ]; then printf 'VITE_API_URL=%s\n' "$VITE_API_URL" >> .env.production.local; fi; \
	if [ -n "${VITE_COGNITO_USER_POOL_ID:-}" ]; then printf 'VITE_COGNITO_USER_POOL_ID=%s\n' "$VITE_COGNITO_USER_POOL_ID" >> .env.production.local; fi; \
	if [ -n "${VITE_COGNITO_APP_CLIENT_ID:-}" ]; then printf 'VITE_COGNITO_APP_CLIENT_ID=%s\n' "$VITE_COGNITO_APP_CLIENT_ID" >> .env.production.local; fi; \
	npm run build

# Production stage
FROM nginx:alpine AS production
COPY --from=build /app/dist /usr/share/nginx/html
COPY Frontend/ExpenseControl.React/docker/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
