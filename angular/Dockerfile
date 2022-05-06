### STAGE 1: Build ###
#LABEL Name=LocationEngine Version=0.0.1
FROM node:12.14-alpine AS build
WORKDIR /usr/src/app
COPY package.json ./
RUN npm install
COPY . .
RUN npm run prod:aws

### STAGE 2: Run ###
FROM nginx:1.17.1-alpine
COPY nginx.conf /etc/nginx/nginx.conf
COPY --from=build /usr/src/app/dist/LocationEngine /usr/share/nginx/html