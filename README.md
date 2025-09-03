# 🔥 Wave Engine Core - Sistema de Controle de Microondas

## 📋 Sobre o Projeto

Sistema completo de controle de microondas desenvolvido em **ASP.NET Core 9.0** seguindo princípios de **Clean Architecture**. Implementa todas as funcionalidades do **Nível 1** conforme especificação técnica, com interface web responsiva e atualizações em tempo real.

## ✨ Funcionalidades Implementadas (Nível 1)

### 🎛️ **Interface e Controles**

- ✅ **Interface web** para configuração de tempo (1-120s) e potência (1-10)
- ✅ **Validação em tempo real** com feedback visual
- ✅ **Potência padrão** pré-configurada em 10
- ✅ **Botões intuitivos** com estados dinâmicos

### ⚡ **Operações de Aquecimento**

- ✅ **Aquecimento personalizado** com tempo e potência configuráveis
- ✅ **Início rápido** (30s, potência 10) com um clique
- ✅ **Acréscimo de tempo** (+30s) durante aquecimento
- ✅ **Conversão automática** de tempo >60s para formato MM:SS (ex: 90s → 1:30)

### 🔄 **Sistema de Pausa e Retomada**

- ✅ **Pausa inteligente** que salva o tempo restante exato
- ✅ **Retomada precisa** do ponto onde parou (sem re-configuração)
- ✅ **Cancelamento total** com limpeza de configurações
- ✅ **Interface adaptativa** que muda conforme o estado

### 📊 **Feedback Visual e Progresso**

- ✅ **String de progresso** baseada na potência (ex: potência 3 = "... ... ...")
- ✅ **Atualização automática** durante aquecimento
- ✅ **Indicador de conclusão** ("Aquecimento concluído")
- ✅ **Tempo formatado** e porcentagem de progresso

### 🛡️ **Validações e Segurança**

- ✅ **Validação de limites** (tempo: 1-120s, potência: 1-10)
- ✅ **Mensagens descritivas** para cada operação
- ✅ **Prevenção de estados inválidos**
- ✅ **Gerenciamento de sessão** robusto

## 🏗️ Arquitetura

### **Estrutura do Projeto**

```
wave-engine-core/
├── Microwave.Domain/           # Entidades e interfaces
│   ├── MicrowaveOven.cs       # Entidade principal
│   ├── IMicrowaveService.cs   # Interface do serviço
│   ├── IStateStorage.cs       # Abstração de persistência
│   └── DTOs/                  # Objetos de transferência
├── Microwave.Application/      # Regras de negócio
│   └── MicrowaveService.cs    # Implementação do serviço
├── Microwave.Infrastructure/   # Infraestrutura
├── Microwave.Tests/           # Testes unitários
└── WaveEngineCore/            # Interface web (Razor Pages)
    ├── Pages/Index.cshtml     # Interface principal
    ├── Infrastructure/        # SessionStateStorage
    └── Program.cs            # Configuração DI
```

### **Padrões Utilizados**

- 🏛️ **Clean Architecture** - Separação clara de responsabilidades
- 🔌 **Dependency Injection** - Inversão de dependências
- 📦 **Repository Pattern** - Abstração de persistência (IStateStorage)
- 🎯 **State Management** - Controle robusto de estados
- 📊 **DTO Pattern** - Transferência estruturada de dados

## 🧪 Testes

### **Cobertura de Testes**

- ✅ **18 testes unitários** com 100% de aprovação
- ✅ **Mock objects** para isolamento de dependências
- ✅ **Cenários críticos** cobertos (validações, estados, progressão)

### **Executar Testes**

```bash
cd wave-engine-core
dotnet test
```

## 🚀 Como Executar

### **Pré-requisitos**

- .NET 9.0 SDK
- IDE/Editor de sua preferência

### **Execução**

```bash
# Clone o repositório
git clone <repository-url>
cd wave-engine-core

# Execute a aplicação
cd WaveEngineCore
dotnet run

# Acesse no navegador
http://localhost:5296
```

## 🎮 Como Usar

### **Operação Básica**

1. **Configure** tempo (1-120s) e potência (1-10)
2. **Inicie** o aquecimento
3. **Monitore** o progresso em tempo real
4. **Pause/Retome** conforme necessário

### **Funcionalidades Avançadas**

- **Início Rápido**: Clique "Início Rápido" para 30s na potência 10
- **Aumento de Tempo**: Use "+30s" durante o aquecimento
- **Atualização Automática**: Ativa automaticamente durante aquecimento
- **Retomada Inteligente**: Após pausar, clique "Retomar Aquecimento"

## 📈 Tecnologias

| Categoria        | Tecnologia                   |
| ---------------- | ---------------------------- |
| **Backend**      | ASP.NET Core 9.0, C# 12      |
| **Frontend**     | Razor Pages, JavaScript ES6+ |
| **Testes**       | xUnit, Mocking               |
| **Arquitetura**  | Clean Architecture, DDD      |
| **Persistência** | Session State (In-Memory)    |

## 🤝 Contribuição

Este projeto demonstra implementação sólida de um teste técnico com arquitetura extensível para futuras funcionalidades, manutenibilidade e boas práticas enterprise.

## 📄 Licença

Este projeto é parte de um teste técnico demonstrativo.

---
