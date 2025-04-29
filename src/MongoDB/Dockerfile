FROM mongo:7.0
RUN useradd -m mongouser
USER mongouser
RUN chown -R mongouser:mongouser /data/db

COPY mongod.conf /etc/mongod.conf
COPY init-mongo.js /docker-entrypoint-initdb.d/
CMD ["mongod", "--config", "/etc/mongod.conf"]