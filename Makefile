.PHONY: kafka
kafka:
	docker build -t kafkaimg -f k8s/kafka-image/Dockerfile k8s/kafka-image
	minikube image load kafkaimg
	kubectl apply -f k8s/kafka.yaml

.PHONY: kafka-down
kafka-down:
	kubectl delete -f k8s/kafka.yaml
	kubectl delete pvc -l app=kafka-app

.PHONY: flink
flink:
	kubectl delete --ignore-not-found=true -f k8s/flink.yaml
	docker build -t flinkimg -f ./Examples/Kubernetes/Flink/Dockerfile ./Examples/Kubernetes/Flink
	minikube image load flinkimg
	kubectl apply -f k8s/flink.yaml

.PHONY: flink-down
flink-down:
	kubectl delete -f k8s/flink.yaml

.PHONY: auger
auger:
	kubectl delete --ignore-not-found=true -f k8s/auger.yaml
	docker build --no-cache -t augerimg -f ./Examples/Kubernetes/CloudNative.Workload/Dockerfile .   
	minikube image load augerimg
	kubectl apply -f k8s/auger.yaml

.PHONY: auger-down
auger-down:
	kubectl delete -f k8s/auger.yaml

.PHONY: company
company: 
	docker build -t companyimg -f ./k8s/company/Dockerfile ./k8s/company
	minikube image load companyimg
	kubectl apply -f k8s/company.yaml

.PHONY: company-down
company-down:
	kubectl delete -f k8s/company.yaml

.PHONY: jsondumper
jsondumper: 
	docker build -t jsondumperimg -f ./k8s/jsondumper/Dockerfile ./k8s/jsondumper
	minikube image load jsondumperimg
	kubectl apply -f k8s/jsondumper.yaml

.PHONY: jsondumper-down
jsondumper-down:
	kubectl delete -f k8s/jsondumper.yaml

.PHONY: generate
generate:
	kubectl delete --ignore-not-found=true -f k8s/generator/job.yaml
	docker build --no-cache -t generatorimg -f ./k8s/generator/Dockerfile ./k8s/generator
	minikube image load generatorimg
	kubectl apply -f k8s/generator/job.yaml

.PHONY: clean
clean: kafka-down auger-down company-down jsondumper-down flink-down

.PHONY: bench-auger
bench-auger: kafka company generate auger

.PHONY: bench-flink
bench-flink: kafka company generate flink

.PHONY: cluster-3cpu-4gb
cluster-3cpu-4gb:
	minikube start --nodes 3 --kubernetes-version v1.31 --addons dashboard,metrics-server --cpus 3 --memory 4g

.PHONY: cluster-10cpu-16gb
cluster-10cpu-16gb:
	minikube start --nodes 3 --kubernetes-version v1.31 --addons dashboard,metrics-server --cpus 10 --memory 16g

.PHONY: cluster-down
cluster-down:
	minikube delete