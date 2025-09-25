

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
  "Key": *Uma string muito longa*,
  "Issuer": "AgendaProApi",
  "Audience": "AgendaProClients",
  "DurationMinutes": *Validade do token*
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

