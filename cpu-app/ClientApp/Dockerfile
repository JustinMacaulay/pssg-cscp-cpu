# Create build image layer
FROM node:8.9.4 AS build

COPY ./cpu-app/ClientApp ./cpu-app/ClientApp

WORKDIR /cpu-app/ClientApp

RUN npm install
RUN npm run build -- --base-href /coastcontracts --prod

# Create runtime image layer, copy build image contents into this image
FROM caddy:latest AS final
COPY ./cpu-app/ClientApp/Caddyfile /etc/caddy/Caddyfile
COPY --from=build /cpu-app/ClientApp/dist/ /site
ENV BASE_URL=
ENV API_URL=
EXPOSE 2015
