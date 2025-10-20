CREATE TABLE countries (
    country_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE sports (
    sport_id SERIAL PRIMARY KEY,
    name VARCHAR(120) NOT NULL UNIQUE,
    is_team BOOLEAN NOT NULL DEFAULT FALSE,
    description TEXT
);

CREATE TABLE venues (
    venue_id SERIAL PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    location VARCHAR(200) NOT NULL
);

CREATE TABLE participants (
    participant_id SERIAL PRIMARY KEY,
    country_id INT NOT NULL REFERENCES countries(country_id) ON DELETE RESTRICT,
    sport_id INT NOT NULL REFERENCES sports(sport_id) ON DELETE RESTRICT,
    full_name VARCHAR(150) NOT NULL,
    birth_date DATE NOT NULL CHECK (birth_date <= CURRENT_DATE),
    gender CHAR(1) CHECK (gender IN ('M','F')),
    UNIQUE (country_id, full_name)
);

CREATE TABLE schedule (
    schedule_id SERIAL PRIMARY KEY,
    sport_id INT NOT NULL REFERENCES sports(sport_id) ON DELETE CASCADE,
    venue_id INT NOT NULL REFERENCES venues(venue_id) ON DELETE CASCADE,
    start_date DATE NOT NULL,
    start_time TIME NOT NULL,
    CHECK (start_date >= DATE '2020-01-01')
);

CREATE TABLE results (
    result_id SERIAL PRIMARY KEY,
    sport_id INT NOT NULL REFERENCES sports(sport_id) ON DELETE CASCADE,
    participant_id INT NOT NULL REFERENCES participants(participant_id) ON DELETE CASCADE,
    place INT CHECK (place > 0),
    score NUMERIC(8,2) DEFAULT 0 CHECK (score >= 0),
    UNIQUE (sport_id, participant_id)
);

CREATE INDEX idx_participants_country ON participants(country_id);
CREATE INDEX idx_participants_sport ON participants(sport_id);
CREATE INDEX idx_results_place ON results(place);
CREATE INDEX idx_schedule_date ON schedule(start_date);
CREATE INDEX idx_results_participant ON results(participant_id);

CREATE OR REPLACE VIEW v_countries AS
SELECT country_id, name
FROM countries;

CREATE OR REPLACE VIEW v_participants_full AS
SELECT p.participant_id,
       p.full_name,
       p.gender,
       p.birth_date,
       c.country_id,
       c.name AS country_name,
       s.sport_id,
       s.name AS sport_name,
       s.is_team
FROM participants p
JOIN countries c ON p.country_id = c.country_id
JOIN sports s ON p.sport_id = s.sport_id;

CREATE OR REPLACE VIEW v_results_summary AS
SELECT c.country_id,
       c.name AS country_name,
       s.sport_id,
       s.name AS sport_name,
       COUNT(r.result_id) AS participants_count,
       SUM(CASE WHEN r.place = 1 THEN 1 ELSE 0 END) AS gold_medals,
       SUM(CASE WHEN r.place = 2 THEN 1 ELSE 0 END) AS silver_medals,
       SUM(CASE WHEN r.place = 3 THEN 1 ELSE 0 END) AS bronze_medals
FROM results r
JOIN participants p ON r.participant_id = p.participant_id
JOIN countries c ON p.country_id = c.country_id
JOIN sports s ON p.sport_id = s.sport_id
GROUP BY c.country_id, c.name, s.sport_id, s.name
HAVING COUNT(r.result_id) > 0;

INSERT INTO countries (name) VALUES
('CountryA'), ('CountryB')
ON CONFLICT DO NOTHING;

INSERT INTO sports (name, is_team, description) VALUES
('100m sprint', FALSE, 'Individual sprint'),
('Relay 4x100', TRUE, 'Team relay')
ON CONFLICT DO NOTHING;

INSERT INTO venues (name, location) VALUES
('Main Stadium', 'City Center'),
('Aquatics Center', 'West Side')
ON CONFLICT DO NOTHING;

INSERT INTO participants (country_id, sport_id, full_name, birth_date, gender)
SELECT c.country_id, s.sport_id, 'Ivan Ivanov', '1995-03-12', 'M'
FROM countries c, sports s
WHERE c.name = 'CountryA' AND s.name = '100m sprint'
ON CONFLICT DO NOTHING;

INSERT INTO participants (country_id, sport_id, full_name, birth_date, gender)
SELECT c.country_id, s.sport_id, 'Petr Petrov', '1993-06-20', 'M'
FROM countries c, sports s
WHERE c.name = 'CountryB' AND s.name = '100m sprint'
ON CONFLICT DO NOTHING;

INSERT INTO schedule (sport_id, venue_id, start_date, start_time)
SELECT s.sport_id, v.venue_id, DATE '2025-07-20', TIME '10:00'
FROM sports s, venues v
WHERE s.name = '100m sprint' AND v.name = 'Main Stadium'
ON CONFLICT DO NOTHING;

INSERT INTO results (sport_id, participant_id, place, score)
SELECT s.sport_id, p.participant_id, 1, 9.85
FROM sports s, participants p
WHERE s.name = '100m sprint' AND p.full_name = 'Ivan Ivanov'
ON CONFLICT DO NOTHING;

INSERT INTO results (sport_id, participant_id, place, score)
SELECT s.sport_id, p.participant_id, 2, 10.12
FROM sports s, participants p
WHERE s.name = '100m sprint' AND p.full_name = 'Petr Petrov'
ON CONFLICT DO NOTHING;
