# Registry et Kubernetes

Maintenant que l'application est prête nous allons voir comment héberger les images ainsi que les déployer sur le cluster Kubernetes.

## Docker Repository

Pour héberger nos images, nous nous servirons de la registry docker hub, qui est celle par défaut fournie par Docker.

### Identification

Afin de pouvoir vous identifier sur le docker hub, il va falloir vous enregistrer. Il vous suffit d'aller sur [https://hub.docker.com](https://hub.docker.com) et vous devriez tomber sur le formulaire d'inscription. ![inscription docker hub](assets/etape-2-docker-hub-register.png)

Une fois fait, il vous reste l'étape de **login** depuis votre terminal en saisissant l'identifiant précédemment choisi.

`docker login -u VOTRE-USERNAME`

```sh
>  docker login -u VOTRE-USERNAME
Password:
Login Succeeded
```

### Envoyer les images

Pour pouvoir envoyer des images sur le docker hub, il faut au préalable les **tagger** en suivant cette nomenclature **VOTRE-USERNAME/NOM-DE-L-IMAGE**, ce qui dans mon cas pour **lab-survey-front** donne `docker tag lab-survey-front riges/lab-survey-front`.

```sh
> docker tag lab-survey-front riges/lab-survey-front
```

Après, il vous suffit de **push** les images en se servant de leurs tags. Cela devrait être 'docker push riges/lab-survey-front'.

```sh
> docker push riges/lab-survey-front
The push refers to repository [docker.io/riges/lab-survey-front]
b55cb9ee1e35: Pushed
5efc006b5ed6: Mounted from library/nginx
b0efd61aab3d: Mounted from library/nginx
fd8dbe3c801b: Mounted from library/nginx
cd7100a72410: Mounted from library/redis
latest: digest: sha256:2d0dd3bc0b2f16725262ab2e6ccaad297c2ba826598a3ed3b980dbd62c7f1495 size: 1364
```

Pour vérifier, allez sur [https://hub.docker.com](https://hub.docker.com). Vous devriez tomber sur vos images :
![images dans le docker hub](assets/etape-2-docker-hub-list.png)

## Kubernetes Dashboard

Le projet Kubernetes Dashboard est un exemple d'implémentation de dashboard web, fourni par les équipes de Kubernetes, qui vous sera très pratique afin de suivre l'avancer des déploiements et du cycle de vie de l'application pendant notre session.

Pour cela, on va **appliquer** un manifeste fourni par le projet afin de déployer les ressources nécessaires au bon fonctionnement du dashboard en précisant que nous voulons appliquer un fichier (**-f**) comme suit : `kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/master/src/deploy/recommended/kubernetes-dashboard.yaml`

```sh
> kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/master/src/deploy/recommended/kubernetes-dashboard.yaml
secret "kubernetes-dashboard-certs" created
serviceaccount "kubernetes-dashboard" created
role "kubernetes-dashboard-minimal" created
rolebinding "kubernetes-dashboard-minimal" created
deployment "kubernetes-dashboard" created
service "kubernetes-dashboard" created
```

Les ressources du cluster étant sur un réseau privé, il vous faut une passerelle permettant de communiquer entre votre machine et ce cluster. Cela tombe bien car **kubectl** fournit une commande permettant de faire un **proxy** : `kubectl proxy`.

```sh
> kubectl proxy
Starting to serve on 127.0.0.1:8001
```

À partir de maintenant, vous avez un proxy sur l'adresse local [127.0.0.1:8001](127.0.0.1:8001) et donc si vous allez sur [http://127.0.0.1:8001/ui](http://127.0.0.1:8001/ui) vous devriez tomber sur cet écran ![Dashboard login](assets/etape-2-dashboard-login.png)

Cet écran est dû aux nouveautés sur les versions de Kubernetes 1.8 et + offrant des notions de sécurisation, il nous faudra donc un compte permettant d'accéder aux Dashboard. Pour cela nous créerons une ressource de type **ServiceAccount** dans le namespace **kube-system**, où sont rassemblés les ressources liées au système, qu'on nommera pour l'occasion **admin-system**.

```yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: admin-user
  namespace: kube-system
```

Une fois le compte créé, il faudra lui créer une ressource de type **ClusterRoleBinding** qui permettra de déclarer les droits à associer au compte **admin-user**. Comme le dashboard nécessite de pouvoir gérer le cluster on déclarera un droit de type **ClusterRole** qui se nomme **cluster-admin**.

```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: admin-user
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: cluster-admin
subjects:
- kind: ServiceAccount
  name: admin-user
  namespace: kube-system
```

Pour cet exemple, nous aurons mis dans le fichier : **dashboard-account.yaml**. Cependant, il pourrait s'appeler comme vous le souhaitez. Une fois le fichier créé, il vous faudra utiliser la fonction **create** de la commande kubectl en lui spécifiant le fichier avec le paramètre **-f** `kubectl create -f dashboard-account.yaml`

> #### Information 💡
>
> Comme nous voulons que cela soit déployé en même temps, nous mettrons cela dans le même fichier, _dashboard-account.yaml_, tout en veillant à bien séparer les manifestes par **---**.

```sh
> kubectl apply -f dashboard-create.yml
serviceaccount "admin-user" created
clusterrolebinding "admin-user" created
```

Maintenant que nous avons créé le compte, il nous faut récupérer le token. Pour cela nous listeront les ressources de type **secret** avec la commande **get**. Si vous vous souvenez bien, le compte a été créé dans le namespace **kube-system**, il faut donc le préciser grâce au paramètre -n. Cela donnerait `kubectl -n kube-system get secret`.

```sh
> kubectl -n kube-system get secret
NAME                                             TYPE                                  DATA      AGE
admin-user-token-5mctg                           kubernetes.io/service-account-token   3         5d
attachdetach-controller-token-pccpr              kubernetes.io/service-account-token   3         5d
bootstrap-signer-token-bqtr7                     kubernetes.io/service-account-token   3         5d
certificate-controller-token-wk974               kubernetes.io/service-account-token   3         5d
clusterrole-aggregation-controller-token-d96ks   kubernetes.io/service-account-token   3         5d
cronjob-controller-token-w279h                   kubernetes.io/service-account-token   3         5d
daemon-set-controller-token-rghdz                kubernetes.io/service-account-token   3         5d
default-token-j8fcr                              kubernetes.io/service-account-token   3         5d
deployment-controller-token-8vtfk                kubernetes.io/service-account-token   3         5d
disruption-controller-token-j2r7j                kubernetes.io/service-account-token   3         5d
endpoint-controller-token-p7qd7                  kubernetes.io/service-account-token   3         5d
generic-garbage-collector-token-mhd4c            kubernetes.io/service-account-token   3         5d
horizontal-pod-autoscaler-token-vvrjt            kubernetes.io/service-account-token   3         5d
job-controller-token-jxfl5                       kubernetes.io/service-account-token   3         5d
kube-dns-token-9ww4v                             kubernetes.io/service-account-token   3         5d
kube-proxy-token-b7jlw                           kubernetes.io/service-account-token   3         5d
```

Il faut chercher le secret qui commence par `admin-user` (**admin-user-token-5mctg** dans notre exemple) puis nous utiliserons la commande **describe** afin de connaître le token. Pour cette commande, on doit préciser le type de resource, **secret** dans notre cas, ainsi que du nom de la ressource. Pour cette exemple, cela donnerait `kubectl -n kube-system describe secret admin-user-token-5mctg`

```sh
> kubectl -n kube-system describe secret admin-user-token-5mctg
Name:         admin-user-token-5mctg
Namespace:    kube-system
Labels:       <none>
Annotations:  kubernetes.io/service-account.name=admin-user
              kubernetes.io/service-account.uid=bd18da92-4244-11e8-b486-00155d019361

Type:  kubernetes.io/service-account-token

Data
====
ca.crt:     1025 bytes
namespace:  11 bytes
token:      eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJrdWJlcm5ldGVzL3NlcnZpY2VhY2NvdW50Iiwia3ViZXJuZXRlcy5pby9zZXJ2aWNlYWNjb3VudC9uYW1lc3BhY2UiOiJrdWJlLXN5c3RlbSIsImt1YmVybmV0ZXMuaW8vc2VydmljZWFjY291bnQvc2VjcmV0Lm5hbWUiOiJhZG1pbi11c2VyLXRva2VuLTVtY3RnIiwia3ViZXJuZXRlcy5pby9zZXJ2aWNlYWNjb3VudC9zZXJ2aWNlLWFjY291bnQubmFtZSI6ImFkbWluLXVzZXIiLCJrdWJlcm5ldGVzLmlvL3NlcnZpY2VhY2NvdW50L3NlcnZpY2UtYWNjb3VudC51aWQiOiJiZDE4ZGE5Mi00MjQ0LTExZTgtYjQ4Ni0wMDE1NWQwMTkzNjEiLCJzdWIiOiJzeXN0ZW06c2VydmljZWFjY291bnQ6a3ViZS1zeXN0ZW06YWRtaW4tdXNlciJ9.ZA8FQ5DUMmhpsSyvcK_w0gObxU2WKNRptYzoBQEinwt6smtK6JygFbJZVmngB9v3sjfBeWLcHr7B79c9TTvhmIc8xfCGcgw4PMyKlMN3U97JC82oNK4GtS3REHZ72K_j7JEBUHsX-m6EfjTt74cmSXpE5evBciHOuAaCg4F0uzi4xf54d9a4F_Gr32VuZ_62EhgMKQ7XQrSZ16d7uGMIsway1S_0lp6WC9Fh9KUbMWkKFDv6b1vPJdn2zPkiIKDxJFtObBhad6Ccux_K-0MolaA4JvXjsLmrPa3rWocSRziCN3vvw5cbqLPGR1400Z2eZq2zSMK1xrnSoy8vLBKjIg
```

En récupérent le token et retournant sur l'écran de login, cela devrait donc donner ces deux étapes ![Dashboard login avec token](assets/etape-2-dashboard-login-with-token.png) ![Dashboard](assets/etape-2-dashboard.png)

## Deploiement de lab-surbey-redis

Pour qu'une application soit déployée et soit accessible hors du pod sur Kubernetes, il faut déclarer deux ressources : un **Deployment** et un **Service**.

### Deploiement du back

Pour le déployment on donnera comme metadata :

- un nom **lab-surbey-redis** afin d'identifier le back.

La partie **spec** correspond aux configurations de votre pod, par le champ **replicat** qui permettra de définir le nombre d'instance qu'y devront être déployées par l'orchestrateur et le champs **template** qui, un peu comme un docker-compose, définit l'application.

Ce **template** comporte des **metadata**, permettant de donner un label qui permettra au service de matcher avec le pod, et à nouveau un champs **spec** où on pourra définir le container. En prenant exemple sur le docker compose, nous le nommerons **lab-surbey-redis**. On prendra l'image **redis** et on précisera qu'on expose le **port** de redis (**6379**) du container grâce à **containerPort: 6379**.

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: lab-survey-redis
spec:
  replicas: 1
  selector:
    matchLabels:
      app: lab-survey-redis
  template:
    metadata:
      labels:
        app: lab-survey-redis
    spec:
      containers:
      - name: lab-survey-redis
        image: redis
        ports:
        - containerPort: 6379
```

### Exposé le back

Pour pouvoir exposer cette application de manière publique, ou entre deux pods, il faut lui déclarer un service. Pour ce faire, il faut déclarer un _kind_ de type **service**, puis lui donner un nom **lab-survey-redis**. Ensuite, nous définirons les **spec** du service :

- le port exposé, **6379**.
- Il faut aussi définir quel service utiliser. Avec Kubernetes la bonne manière est de se baser sur les labels afin de matcher les services à une application. Dans ce cas, il faut utiliser le paramètre **selector** et nous lui donnerons donc simplement la règle **app: lab-survey-redis** qui est celle définie dans notre déploiement.

Le service par défaut sur Kubernetes est le _ClusterIP_, cela permet d'exposer un ou des pods (par répartition de charge) à d'autre à l'intérieur du cluster.

```yaml
apiVersion: v1
kind: Service
metadata:
  name: lab-survey-redis
spec:
  ports:
  - port: 6379
  selector:
    app: lab-survey-redis
```

Nous mettrons ces deux manifestes dans un fichier, que nous nommerons _lab-survey-redis.yaml_ et nous créerons les ressources grâce à la commande create en spécifiant le fichier avec le paramètre **-f**.

```sh
$ kubectl create -f lab-survey-redis.yaml
deployment "lab-survey-redis" created
service "lab-survey-redis" created
```

## Deploiement lab-survey-front

### Le déploiement du front

Il faut maintenant préparer le manifest du déploiement, nous prendrons donc **lab-survey-front** comme nom.

En plus du _template_ du pod, nous allons alors spécifier que nous ne voulons qu'une instance de ce pod, **replicas: 1**, et que la stratégie de mise à jour, en mise à jour part lot (**rollingUpdate**), sera de mettre à jour avec seulement une seule instance indisponible et en déployant une instance à la fois.

La partie _spec_ comporte la définition du _containers_ que nous configurerons comme suit :

- Un nom, qui sera donc **lab-survey-front**.
- l'**image** docker que nous utiliserons. Nous prendrons donc, l'image **VOTRE-USERNAME/lab-survey-front**.
- Comme pour le _docker-compose.yaml_ nous donnons deux variables d'environement (**env**) :
  - **REDIS** avec comme valeur le nom du serveur exposant le back (**lab-survey-redis**)
  - **ASPNETCORE_ENVIRONMENT** qui contiendra **Production** pour que l'application prenne la bonne configuration.
- Pour les _ports_, nous exposerons le port **5000**, qu'utilise l'application en .Net Core.

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: lab-survey-front
spec:
  replicas: 1
  strategy:
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 1
  selector:
    matchLabels:
      app: lab-survey-front
  template:
    metadata:
      labels:
        app: lab-survey-front
    spec:
      containers:
      - name: lab-survey-front
        image: riges/lab-survey-front
        env:
        - name: REDIS
          value: "lab-survey-redis"
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        ports:
        - containerPort: 5000
```

### Exposition public du Front

Maintenant que le pod du front est fait, il faut s'occuper du service qui nous permettra d'exposer le front. Pour cela, on refait un service comme sur le redis et le _selector_ sera **app: lab-survey-front**, permettant de trouver les pods du front. La partie changeante sera celle sur le port, que nous nommerons **http**, car nous souhaitons déclarer une redirection du port **80** vers le port **5000** de l'application.

```yaml
apiVersion: v1
kind: Service
metadata:
  name: lab-survey-front
spec:
  ports:
  - name: http
    port: 80
    targetPort: 5000
  selector:
    app: lab-survey-front
```

Comme pour le back, nous réunirons les manifests en un fichier _lab-survey-front.yaml_ que nous passerons à `kubectl create` grâce au paramètre **-f**.

```sh
$ kubectl create -f lab-survey-front.yaml
deployment "lab-survey-front" created
service "lab-survey-front" created
```

### Test du deploiement

Dans un cas plus "normal", nous aurions déclaré le service front en _type_ **loadbalancer** afin de rendre accessible l'application en dehors du cluster. Mais malheureusement, par défaut Kubernetes ne fournit pas ce type de ressource sur un cluster "local". Si vous installez cela sur un cluster géré par un Rancher Labs, ou un cloud provider (Google Cloud Plateforme, Azure, AWS, ...) il vous suffirait de rajouter l'information de type au manifeste du service et vous auriez une ip externe pour tester votre application. À défaut de meilleure solution, nous allons voir comment tester cela grâce à la commande **proxy**.

Pour cela, si votre commande `kubectl proxy` tourne toujours, il vous suffit d'utiliser la syntaxe : http://localhost:8001/api/v1/proxy/namespaces/default/services/**SERVICE-NAME**:**SERVICE-PORT-NAME-OU-NUMEROS-DE-PORT**/

Il faut donc aller sur [http://localhost:8001/api/v1/proxy/namespaces/default/services/lab-survey-front:http/](http://localhost:8001/api/v1/proxy/namespaces/default/services/lab-survey-front:http/) et nous devrions arriver sur l'écran ci-dessous.![retour lab-survey-front](assets/etape-2-lab-survey-front-preview-proxy.png)

## Félicitation, vous avez déployé votre premiere application sur Kubernetes. 🎊🏆🎉

Si vous êtes là, normalement, vous avez déployé votre première application sur Kubernetes tout en permettant de découpler les ressources de l'application, un serveur redis et votre code.
