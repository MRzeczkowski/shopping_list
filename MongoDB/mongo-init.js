db.createUser(
    {
        user: "productsApi",
        pwd: "Pa$$w0rd!",
        roles: [
            {
                role: "readWrite",
                db: "productsDB"
            }
        ]
    }
);

db = db.getSiblingDB('productsDB');

db.products.insertMany([
    { Name: "Kefir", IsPurchased: true },
    { Name: "Bugatti", IsPurchased: false },
]);
