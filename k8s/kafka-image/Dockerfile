# Based on https://arifkiziltepe.medium.com/kubernetes-kafka-cluster-install-without-zookeeper-9649efeefaf9
FROM openjdk:17-bullseye

ENV KAFKA_VERSION=3.9.0
ENV SCALA_VERSION=2.13
ENV KAFKA_HOME=/opt/kafka
ENV PATH=${PATH}:${KAFKA_HOME}/bin
LABEL name="kafka" version=${KAFKA_VERSION}

RUN wget -O /tmp/kafka_${SCALA_VERSION}-${KAFKA_VERSION}.tgz https://downloads.apache.org/kafka/${KAFKA_VERSION}/kafka_${SCALA_VERSION}-${KAFKA_VERSION}.tgz \
    && tar xfz /tmp/kafka_${SCALA_VERSION}-${KAFKA_VERSION}.tgz -C /opt \
    && rm /tmp/kafka_${SCALA_VERSION}-${KAFKA_VERSION}.tgz \
    && ln -s /opt/kafka_${SCALA_VERSION}-${KAFKA_VERSION} ${KAFKA_HOME} \
    && rm -rf /tmp/kafka_${SCALA_VERSION}-${KAFKA_VERSION}.tgz

COPY ./entrypoint.sh /

RUN ["chmod", "+x", "/entrypoint.sh"]
ENTRYPOINT ["/entrypoint.sh"]
