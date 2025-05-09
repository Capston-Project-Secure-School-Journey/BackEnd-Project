FROM nginx:1-alpine


COPY nginx.conf /etc/nginx/nginx.conf
COPY NotFoundPage.html /var/www/html/
COPY fullchain.pem /etc/letsencrypt/live/ssast.tech/fullchain.pem
COPY privkey.pem /etc/letsencrypt/live/ssast.tech/privkey.pem
EXPOSE 80
EXPOSE 443

CMD ["nginx", "-g", "daemon off;"]
