FROM node:8-alpine AS builder
WORKDIR /app
COPY . .
RUN yarn install
RUN yarn run build

FROM nginx:alpine
COPY --from=builder /app/build /usr/share/nginx/html
