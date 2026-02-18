# Build stage
FROM node:20-alpine AS build
WORKDIR /app
COPY Frontend/ExpenseControl.React/package*.json ./
RUN npm ci
COPY Frontend/ExpenseControl.React/. .
RUN npm run build

# Production stage
FROM nginx:alpine AS production
COPY --from=build /app/dist /usr/share/nginx/html
COPY Frontend/ExpenseControl.React/docker/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
