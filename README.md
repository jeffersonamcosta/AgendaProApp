

# 📅 AgendaPro

AgendaPro é um sistema de **gerenciamento de eventos corporativos** que integra **participantes, fornecedores e serviços**.
O projeto é dividido em duas partes:

* ⚙️ **[AgendaPro](https://github.com/jeffersonamcosta/AgendaPro/) (Backend – ASP.NET Core Web API)** → API responsável pelas rotas, regras de negócio e persistência no banco.
* 🖥️ **[AgendaProApp](https://github.com/jeffersonamcosta/AgendaProApp/) (Frontend – WPF .NET)** → Aplicação desktop que consome a API e permite interação gráfica.

---

## 📂 Estrutura do Projeto

```
.
├── AgendaPro/                #(Backend) API ASP.NET Core
│   ├── Controllers/          # Controllers da API (Participantes, Eventos, Fornecedores, Relatórios...)
│   ├── Data/                 # DbContext e configuração de banco
│   ├── Models/               # Entidades (Evento, Participante, Serviço, etc.)
│   ├── Properties/           # Configurações de publicação
│   ├── Views/                # Páginas MVC
│   ├── wwwroot/              # Arquivos estáticos (css, js, bootstrap, etc.)
│   ├── Program.cs            # Configuração principal da API
│   ├── appsettings.json      # Configuração de banco e JWT
│  
│
├── AgendaProApp/             #(Frontend) Aplicação WPF (.NET 8)
│   ├── MainWindow.xaml       # Janela de login
│   ├── Principal.xaml        # Janela principal
│   ├── Principal.xaml.cs     # Lógica principal
│   ├── Principal.*.cs        # Arquivos de apoio Lógica principal
│   ├── APIAgendaPro.cs       # Classe de integração com a API
│   ├── Properties/           # Configurações do projeto WPF
│   
│
└── .github/workflows/        # CI/CD (build/test/deploy)
```

---

## ⚙️ Dependências

### 🔗 Backend (API – AgendaPro)

* 🟦 [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
* 🗄️ [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
* 💾 [Banco de dados SQL Server 2022 ou +](https://www.microsoft.com/pt-br/sql-server/sql-server-downloads/)

### 🖥️ Frontend (WPF – AgendaProApp)

* 🟦 [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
* 💻 Windows (necessário para WPF)
* 📦 `System.Text.Json` para serialização de dados
* 🌐 `HttpClient` para comunicação com a API

---

## ▶️ Como Rodar o Projeto

### ⬇️ 1. Baixar os arquivos da publicação

📦 [Baixe o Arquivo](https://drive.google.com/uc?export=download&id=1xJ_8EwqDIdd2DgT2BXbEjPnXPGpMRH7X) e extraia no disco local `C:\`

---

### 🗄️ 2. Banco de dados

#### 🔹 2.1 (Alternativa) - Restaurar Banco de teste

Restaure:

```
\AgendaPro\Banco de dados\AgendaPro_bd_Teste.bak
```

#### 🔹 2.2 (Alternativa) - Criar novo OU Restaurar banco 0

Na pasta `AgendaPro\Banco de dados` possui o arquivo `AgendaPro_bd_0.bak` que contém o banco de dados com suas tabelas mas **sem informação**.
Também há o script de criação de banco (`Criarbanco.sql`).

---

### ⚙️ 3. Definir arquivo `appsettings.json`

Este arquivo possui as configurações do app.

#### 📌 3.1 - Em `AgendaPro/appsettings.json`

##### 🔧 3.1.1 - Defina as configurações de conexão com banco de dados

Exemplo padrão:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=*Meu PC*\\*Minha Instancia*;Database=*AgendaPro*;User Id=*agendapro*;Password=*senhasecreta*;TrustServerCertificate=True;"
}
```

##### 🌐 3.1.2 - Defina o host e porta em http

Exemplo padrão para funcionar na porta **8080**:

```json
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://*:8080"
    }
  }
}
```

##### 🔑 3.1.3 (Opcional) - Defina as configs de autenticação

Exemplo padrão:

```json
"Jwt": {
  "Key": *UMA STRING MUITO LONGA AQUI*,
  "Issuer": "AgendaProApi",
  "Audience": "AgendaProClients",
  "DurationMinutes": *VALIDADE DO TOKEN*
}
```

---

#### 📌 3.2 - Em `AgendaProApp/appsettings.json`

##### 🌐 3.2.1 - Defina as configurações de conexão com a API

Exemplo padrão para conectar no computador local na porta **8080**:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:8080/api/",
    "BearerToken": ""
  }
}
```

---

### 🔐 4 (Opcional) - Definir usuário de acesso

Se no **Passo 2** você utilizou o **backup de teste**, já existe um usuário padrão:

```
Usuário: admin
Senha:  admin
```

👉 Neste caso, você pode pular esta etapa.

Se você utilizou o **script de criação do banco** (sem dados de teste), será necessário **criar o primeiro usuário manualmente**.

Para isso, utilize a rota **`POST /auth/register`** da API:

* **URL:**

```
http://localhost:5000/auth/register
```

* **Body (JSON):**

```json
{
  "login": "admin",
  "senha": "admin"
}
```

* **Retorno esperado:**

```json
{
  "id": 1,
  "login": "admin",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

Esse **token** será usado automaticamente pela aplicação.

---

### ▶️ 5. Execução

#### 🚀 5.1 (API) No servidor, execute o arquivo:

```
\AgendaPro\AgendaPro.exe
```

O programa irá subir a API na porta configurada no `appsettings.json`.
⚠️ **Deve estar em execução para o sistema funcionar.**

#### 🖥️ 5.2 (AgendaProApp) No endpoint, execute:

```
\AgendaProApp\AgendaProApp.exe
```

O aplicativo irá abrir a interface desktop que consome a API.

---

## 🔑 Autenticação

* O login retorna um **JWT Token**, que é armazenado e utilizado automaticamente pelo `APIAgendaPro`.
* Esse token é necessário para acessar as rotas protegidas (**participantes, eventos, fornecedores, etc.**).

---

---

## 🗂️ Modelo DER do Banco de Dados

Abaixo está o modelo **DER (Diagrama Entidade-Relacionamento)** que representa a estrutura do banco do sistema AgendaPro:

<img width="791" height="739" alt="DER" src="https://github.com/user-attachments/assets/acea66ea-0e6d-4790-9d44-7664f748ac5a" />


---

### 🔎 Exemplos de Uso

#### 📅 Pesquisa de próximos eventos
Para saber o próximo evento de um participante (ou eventos onde **estão os mesmos participantes**), basta:

1. Ir na tela de **Eventos**  
2. Listar os **usuários/participantes**  
3. Marcar os que deseja  
4. Clicar em **Pesquisar**  

<img width="990" height="594" alt="Proximos eventos de um participante" src="https://github.com/user-attachments/assets/5b3b2b8e-ee83-42d0-b529-1e46d24b65a3" />


---

#### 📊 Grid de exibição dinâmica
- A **grid** é dinâmica, então é possível **organizar por qualquer coluna**.  
- A grid também mostra o **tipo de participante** da pesquisa.  
- Todos os campos são pesquisáveis:
  - Se selecionar o **tipo de participante** e clicar em pesquisar → retorna apenas os participantes desse tipo.
#### 👤 Edição de participantes
- Ao selecionar um participante na lista, o sistema já carrega os **campos para edição**.  
- Após ajustar as informações, basta clicar em **Atualizar**.  

<img width="984" height="595" alt="Contagem participantes" src="https://github.com/user-attachments/assets/5eaa4f1e-0107-4f60-9eba-8e0bd7a8416c" />

---

#### 🏢 Fornecedores e serviços
- Um **fornecedor pode ter N serviços**.  
- Para cadastrar um novo serviço:
  1. Escreva o nome e o valor no **grid correspondente**  
  2. Clique em **Atualizar** (ou em **Incluir**, se estiver criando um novo fornecedor).  

<img width="986" height="587" alt="Novo servico fornecedor" src="https://github.com/user-attachments/assets/ce79cce1-2f76-4ea9-86b4-2b7788930c84" />


---

## 🌐 API e Comunicação

Toda comunicação entre o **Frontend (WPF)** e o **Backend (API)** é feita por **requisições REST em JSON**.  
<img width="1488" height="903" alt="Api funciona" src="https://github.com/user-attachments/assets/24598734-3970-4fdf-b64d-c4202e471471" />

👉 Na prática, é possível operar o sistema **somente com o Postman** ou até mesmo desenvolver outra interface gráfica, mantendo o mesmo backend.

### 🔐 Autenticação
- É necessário possuir um **usuário válido** para receber um **token JWT válido**.  
- Sem o token, a API rejeita qualquer ação.  

<img width="1491" height="432" alt="API TOKEN" src="https://github.com/user-attachments/assets/941af3a9-494f-41e5-a3a9-b5e6e395a45e" />

<img width="1495" height="438" alt="sem token" src="https://github.com/user-attachments/assets/68c1bcdb-de8f-466d-9c8d-7291423c5fbf" />


---



## 📊 Funcionalidades Principais

✅ **Participantes** → Cadastro, pesquisa, atualização e Tipos de participantes mais frequentes.
✅ **Fornecedores** → Cadastro, pesquisa, atualização com serviços associados.
✅ **Eventos** → Cadastro de eventos com participantes e serviços relacionados. Consulta de informações de orçamentos/capacidade e data. Também é possível pesquisar os eventos que determinado participante foi convidado.

---

## 📌 Observações

⚠️ Não esqueça de configurar o `appsettings.json` nos **2 programas**.
Se for usar SQL Server local, exemplo:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=AgendaPro;User Id=sa;Password=suasenha;"
}
```

---

👨‍💻 desenvolvido com ❤️ por [**Jefferson Costa**](https://www.linkedin.com/in/jeffersonamcosta)

---
