--
-- TOC entry 203 (class 1259 OID 32829)
-- Name: history; Type: TABLE; Schema: public; Owner: lykkex
--

CREATE TABLE history (
    id uuid NOT NULL,
    wallet_id uuid NOT NULL,
    asset_id text,
    assetpair_id text,
    volume numeric(28,10) NOT NULL,
    type integer NOT NULL,
    create_dt timestamp with time zone NOT NULL,
    context jsonb
);


ALTER TABLE history OWNER TO lykkex;

--
-- TOC entry 204 (class 1259 OID 34656)
-- Name: orders; Type: TABLE; Schema: public; Owner: lykkex
--

CREATE TABLE orders (
    id uuid NOT NULL,
    matching_id uuid NOT NULL,
    wallet_id uuid NOT NULL,
    type integer NOT NULL,
    side integer NOT NULL,
    status integer NOT NULL,
    assetpair_id text NOT NULL,
    volume numeric(28,10) NOT NULL,
    price numeric(28,10),
    create_dt timestamp with time zone NOT NULL,
    register_dt timestamp with time zone NOT NULL,
    status_dt timestamp with time zone NOT NULL,
    match_dt timestamp with time zone,
    remaining_volume numeric(28,10) NOT NULL,
    reject_reason text,
    lower_limit_price numeric(28,10),
    lower_price numeric(28,10),
    upper_limit_price numeric(28,10),
    upper_price numeric(28,10),
    straight boolean NOT NULL,
    sequence_number bigint NOT NULL
);


ALTER TABLE orders OWNER TO lykkex;

--
-- TOC entry 3370 (class 2606 OID 33838)
-- Name: history history_pkey; Type: CONSTRAINT; Schema: public; Owner: lykkex
--

ALTER TABLE ONLY history
    ADD CONSTRAINT history_pkey PRIMARY KEY (id, wallet_id);


--
-- TOC entry 3374 (class 2606 OID 34663)
-- Name: orders orders_pkey; Type: CONSTRAINT; Schema: public; Owner: lykkex
--

ALTER TABLE ONLY orders
    ADD CONSTRAINT orders_pkey PRIMARY KEY (id);


--
-- TOC entry 3371 (class 1259 OID 34646)
-- Name: history_type_pair_date_idx; Type: INDEX; Schema: public; Owner: lykkex
--

CREATE INDEX history_type_pair_date_idx ON public.history USING btree (type, assetpair_id, create_dt DESC);


--
-- TOC entry 3372 (class 1259 OID 34642)
-- Name: history_wallet_type_date_idx; Type: INDEX; Schema: public; Owner: lykkex
--

CREATE INDEX history_wallet_type_date_idx ON public.history USING btree (wallet_id, type, create_dt DESC);


--
-- TOC entry 3375 (class 1259 OID 38161)
-- Name: orders_wallet_type_date_idx; Type: INDEX; Schema: public; Owner: lykkex
--

CREATE INDEX orders_wallet_type_date_idx ON public.orders USING btree (wallet_id, type, create_dt DESC);


-- Completed on 2018-08-08 14:09:30

--
-- PostgreSQL database dump complete
--

