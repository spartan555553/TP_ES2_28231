CREATE TABLE Users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(255),
    password VARCHAR(255),
    user_type VARCHAR(255),
    user_status VARCHAR(255)
);