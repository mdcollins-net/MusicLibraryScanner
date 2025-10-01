-- Drop tables in correct dependency order
DROP TABLE IF EXISTS Tracks CASCADE;
DROP TABLE IF EXISTS Albums CASCADE;
DROP TABLE IF EXISTS Artists CASCADE;

-- ========================
-- Artists Table
-- ========================
CREATE TABLE Artists (
    ID INT GENERATED ALWAYS AS IDENTITY (START WITH 1000 INCREMENT BY 1) PRIMARY KEY,
    Name VARCHAR(255) NOT NULL UNIQUE
);

-- ========================
-- Albums Table
-- ========================
CREATE TABLE Albums (
    ID INT GENERATED ALWAYS AS IDENTITY (START WITH 1000 INCREMENT BY 1) PRIMARY KEY,
    ArtistId INT NOT NULL,
    Year INT,
    Title VARCHAR(255) NOT NULL,
    CONSTRAINT FK_Albums_Artists FOREIGN KEY (ArtistId) REFERENCES Artists(ID),
    CONSTRAINT UQ_Albums UNIQUE (ArtistId, Year, Title)
);

-- ========================
-- Tracks Table
-- ========================
CREATE TABLE Tracks (
    ID INT GENERATED ALWAYS AS IDENTITY (START WITH 1000 INCREMENT BY 1) PRIMARY KEY,
    AlbumId INT NOT NULL,
    ArtistId INT NOT NULL,
    Album VARCHAR(255) NOT NULL,
    Artist VARCHAR(255) NOT NULL,
    Year INT,
    Title VARCHAR(255) NOT NULL,
    TrackNumber INT,
    Duration INT,
    DateTagged TIMESTAMP,
    MusicBrainzTrackId VARCHAR(64),
    MusicBrainzReleaseId VARCHAR(64),
    MusicBrainzArtistId VARCHAR(64),
    CONSTRAINT FK_Tracks_Albums FOREIGN KEY (AlbumId) REFERENCES Albums(ID),
    CONSTRAINT FK_Tracks_Artists FOREIGN KEY (ArtistId) REFERENCES Artists(ID),
    CONSTRAINT UQ_Tracks UNIQUE (AlbumId, TrackNumber, Title)
);
