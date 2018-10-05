CREATE TABLE operations (
    id uuid NOT NULL,
    type integer NOT NULL,
    create_dt timestamp with time zone NOT NULL
);

ALTER TABLE ONLY operations
    ADD CONSTRAINT operations_pkey PRIMARY KEY (id);