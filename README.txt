User UI
 ↓
ChatController (MVC)
 ↓
NLP Engine (TF-IDF + context)
 ↓
Knowledge Base (SQL)
 ↓
Learning Queue (if unknown)
 ↓
Admin Approval
 ↓
Back to KB


1️⃣ Create Database
CREATE DATABASE ExpenseChatBotDB;

CREATE TABLE Chat_Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserName VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(256) NOT NULL,
    DOB DATE NOT NULL,
    Role VARCHAR(20) DEFAULT 'User', -- User / Admin
    CreatedOn DATETIME DEFAULT GETDATE()
);


2️⃣ Knowledge Base Table
CREATE TABLE Chat_KnowledgeBase (
    Id INT IDENTITY PRIMARY KEY,
    Question NVARCHAR(500),
    Answer NVARCHAR(MAX),
    AllowedRole VARCHAR(50) DEFAULT 'Employee',
    IsActive BIT DEFAULT 1,
    CreatedOn DATETIME DEFAULT GETDATE()
);


3️⃣ Learning Queue (Unknown Questions)
CREATE TABLE Chat_LearningQueue (
    Id INT IDENTITY PRIMARY KEY,
    Question NVARCHAR(500),
    UserAnswer NVARCHAR(MAX),
    AskedCount INT DEFAULT 1,
    Status VARCHAR(20) DEFAULT 'Pending', -- Pending / Approved / Rejected
    CreatedOn DATETIME DEFAULT GETDATE()
);

4️⃣ Chat Analytics (Admin Report)
CREATE TABLE Chat_Analytics (
    Id INT IDENTITY PRIMARY KEY,
    Question NVARCHAR(500),
    IsAnswered BIT,
    CreatedOn DATETIME DEFAULT GETDATE(),
    ConfidenceScore FLOAT
);

CREATE TABLE Chat_AuditLog (
    Id INT IDENTITY PRIMARY KEY,
    Question NVARCHAR(500),
    ActionTaken VARCHAR(20), -- Approved / Rejected / Modified
    OldAnswer NVARCHAR(MAX),
    NewAnswer NVARCHAR(MAX),
    ActionBy VARCHAR(100),
    ActionOn DATETIME DEFAULT GETDATE()
);


CREATE TABLE Chat_History (
    Id INT IDENTITY PRIMARY KEY,
    UserId UNIQUEIDENTIFIER,
    Sender VARCHAR(10), -- User / Bot
    Message NVARCHAR(MAX),
    Confidence FLOAT NULL,
    CreatedOn DATETIME DEFAULT GETDATE(),
    SessionId UNIQUEIDENTIFIER
);


CREATE TABLE Chat_Sessions (
    SessionId UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    LoginTime DATETIME DEFAULT GETDATE(),
    LogoutTime DATETIME NULL
);



SELECT TOP 10 Question, COUNT(*) AskedCount
FROM Chat_Analytics
GROUP BY Question
ORDER BY AskedCount DESC;


ALTER TABLE Chat_LearningQueue
ADD
    --AskedCount INT DEFAULT 1,
    ConfidenceScore FLOAT NULL,
    SubmittedBy VARCHAR(100) DEFAULT 'User';