# Grain Auger

Declarative dataflow language over Orleans streams.

## Overview

Grain Auger is a declarative dataflow language over Orleans streams. It allows you to define a dataflow graph using a simple DSL and then execute it on an Orleans cluster. The dataflow graph is defined as a directed acyclic graph (DAG) where each node is a grain and each edge is a virtual stream. The language is designed to be simple and expressive, allowing you to define complex dataflows with minimal effort and boilerplate.

## Related Work

- [Microsoft Orleans](https://learn.microsoft.com/en-us/dotnet/orleans/): Orleans is a framework for building distributed systems in .NET. It provides a simple programming model for building distributed applications using the actor model. Orleans streams are a powerful abstraction for building scalable and fault-tolerant data processing pipelines.
- [Apache Beam](https://beam.apache.org/): Beam is a unified programming model for building batch and streaming data processing pipelines. It provides a high-level API for defining data processing pipelines that can be executed on a variety of distributed processing backends.
- [Apache Flink](https://flink.apache.org/): Flink is a distributed stream processing framework for building real-time data processing applications. It provides a high-level API for defining data processing pipelines that can be executed on a distributed cluster.
