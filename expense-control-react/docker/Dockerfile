# Build stage
FROM node:20-alpine AS build
WORKDIR /app
COPY expense-control-react/package*.json ./
RUN npm ci
COPY expense-control-react/. .
RUN npm run build

# Production stage
FROM nginx:alpine AS production
COPY --from=build /app/dist /usr/share/nginx/html
COPY expense-control-react/docker/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
