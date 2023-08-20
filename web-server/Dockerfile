FROM node:18.17.0

ENV DOCKER=1

WORKDIR /app

COPY . .

RUN npm install

EXPOSE 8080

CMD ["npm", "start"]
