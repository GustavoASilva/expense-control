# Build stage
FROM node:20-alpine AS build
WORKDIR /app
COPY Frontend/ExpenseControl.React/package*.json ./
RUN npm ci
COPY Frontend/ExpenseControl.React/. .
ARG VITE_USE_MOCK_AUTH=false
ARG VITE_API_URL=http://localhost:5293/api
ARG VITE_COGNITO_USER_POOL_ID=
ARG VITE_COGNITO_APP_CLIENT_ID=
ENV VITE_USE_MOCK_AUTH=$VITE_USE_MOCK_AUTH
ENV VITE_API_URL=$VITE_API_URL
ENV VITE_COGNITO_USER_POOL_ID=$VITE_COGNITO_USER_POOL_ID
ENV VITE_COGNITO_APP_CLIENT_ID=$VITE_COGNITO_APP_CLIENT_ID
RUN npm run build

# Production stage
FROM nginx:alpine AS production
COPY --from=build /app/dist /usr/share/nginx/html
COPY Frontend/ExpenseControl.React/docker/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
