CREATE TABLE Assets (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES Users(id),
    asset_type VARCHAR(255),
    start_date DATE,
    duration_in_months INTEGER,
    tax_percentage NUMERIC(5, 2)
);