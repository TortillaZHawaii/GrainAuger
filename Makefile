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
auger: redis
	kubectl delete --ignore-not-found=true -f k8s/auger.yaml
	docker build --no-cache -t augerimg -f ./Examples/Kubernetes/CloudNative.Workload/Dockerfile .   
	minikube image load augerimg
	kubectl apply -f k8s/auger.yaml

.PHONY: auger-down
auger-down:
	kubectl delete -f k8s/auger.yaml

.PHONY: redis
redis:
	kubectl apply -f k8s/redis.yaml

.PHONY: redis-down
redis-down:
	kubectl delete -f k8s/redis.yaml

.PHONY: company
company: 
	docker build -t companyimg -f ./k8s/company/Dockerfile ./k8s/company
	minikube image load companyimg
	kubectl apply -f k8s/company.yaml

.PHONY: company-down
company-down:
	kubectl delete -f k8s/company.yaml

.PHONY: generate
generate:
	kubectl delete --ignore-not-found=true -f k8s/generator/job.yaml
	docker build --no-cache -t generatorimg -f ./k8s/generator/Dockerfile ./k8s/generator
	minikube image load generatorimg
	kubectl apply -f k8s/generator/job.yaml

.PHONY: clean
clean: kafka-down auger-down redis-down
