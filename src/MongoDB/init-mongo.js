db = db.getSiblingDB('SSAST_DB');

db.createUser({
  user: 'admin',
  pwd: '',
  roles: [
    {
      role: 'dbOwner',
      db: 'SSAST_DB'
    },
    { role: "dbOwner", db: "admin" },
    { role: "dbOwner", db: "config" },
    { role: "dbOwner", db: "local" }
  ]
});
