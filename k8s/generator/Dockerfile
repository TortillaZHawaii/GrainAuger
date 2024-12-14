FROM golang:1.23.3-alpine AS builder

WORKDIR /app
COPY . .
RUN go build -ldflags "-s -w" -o /app/generator

FROM alpine:3.15

COPY --from=builder /app/generator /app/generator

ENTRYPOINT ["/app/generator"]
