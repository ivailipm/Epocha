FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine AS runtime
# Matches the docker-compose service name; a real deployment overrides it (e.g. Fly's internal
# hostname for the API app). The base image's own entrypoint substitutes it into the config at
# container startup, so no code change is needed to point this at a different API.
ENV API_ORIGIN=http://api:8080
COPY nginx.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
