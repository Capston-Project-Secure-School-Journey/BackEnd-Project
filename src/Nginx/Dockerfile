FROM nginx:1-alpine


COPY nginx.conf /etc/nginx/nginx.conf
COPY NotFoundPage.html /var/www/html/
COPY api.fullchain.pem /etc/letsencrypt/live/api.ssast.tech/fullchain.pem
COPY api.privkey.pem /etc/letsencrypt/live/api.ssast.tech/privkey.pem
COPY dashboard.fullchain.pem /etc/letsencrypt/live/dashboard.ssast.tech/fullchain.pem
COPY dashboard.privkey.pem /etc/letsencrypt/live/dashboard.ssast.tech/privkey.pem
EXPOSE 80
EXPOSE 443

CMD ["nginx", "-g", "daemon off;"]
