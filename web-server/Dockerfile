FROM node:18.17.0

WORKDIR /app

COPY . .

ENV DOCKER=1

RUN npm install

EXPOSE 8080

CMD ["npm", "start"]
