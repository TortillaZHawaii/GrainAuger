ARG MAVEN_VERSION=3-eclipse-temurin-11
ARG FLINK_VERSION=1.20-java11

FROM maven:${MAVEN_VERSION} AS build
WORKDIR /app

COPY pom.xml .
COPY src src

RUN mvn -f pom.xml clean package

FROM flink:${FLINK_VERSION}

COPY --from=build /app/target/flink-k8s-example-1.0-SNAPSHOT.jar /app.jar

CMD ["standalone-job", "--job-classname", "workload.BenchmarkJob"]
