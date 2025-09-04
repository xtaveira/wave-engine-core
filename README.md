# 🎛️ Wave Engine Core

Sistema de controle de microondas em **ASP.NET Core 9.0** com **Clean Architecture**.

## 🚀 Como Executar

```bash
# Clone e execute
git clone <repository-url>
cd wave-engine-core
dotnet run --project WaveEngineCore

# Acesse: http://localhost:5296
```

## ✨ Funcionalidades Implementadas

### 🥇 **Nível 1 - Básico**

- ✅ **Aquecimento manual** (tempo 1-120s, potência 1-10)
- ✅ **Aquecimento rápido** (30s na potência 10)
- ✅ **Pausar/Retomar** aquecimento
- ✅ **Adicionar tempo** (+30s durante aquecimento)
- ✅ **String de progresso** visual com caracteres

### 🥈 **Nível 2 - Programas Pré-definidos**

- ✅ **Pipoca** (180s, potência 7) - ∩
- ✅ **Leite** (300s, potência 5) - ∿
- ✅ **Carnes de boi** (840s, potência 4) - ≡
- ✅ **Frango** (480s, potência 7) - ∴
- ✅ **Feijão** (480s, potência 9) - ◊

### 🥉 **Nível 3 - Programas Personalizados**

- ✅ **Criar programas** customizados (CRUD completo)
- ✅ **Editar/Excluir** programas existentes
- ✅ **Validação avançada** (tempo 1-7200s, caracteres únicos)
- ✅ **Persistência JSON** thread-safe
- ✅ **Interface unificada** (pré-definidos + customizados)

### 🏆 **Nível 4 - Web API & Autenticação**

- ✅ **Web API REST** completa (20 endpoints)
  - **Autenticação**: 5 endpoints (`/api/auth/*`)
  - **Aquecimento**: 8 endpoints (`/api/microwave/heating/*`)
  - **Programas**: 7 endpoints (`/api/microwave/programs/*`)
- ✅ **JWT Bearer Authentication** (8h expiração)
- ✅ **Sistema de exceções** customizado + middleware
- ✅ **Criptografia SHA1 + AES** para segurança
- ✅ **Logging centralizado** thread-safe

## 🏗️ Arquitetura

```
📁 Microwave.Domain/          # Entidades e regras de negócio
📁 Microwave.Application/     # Serviços da aplicação
📁 Microwave.Infrastructure/  # Persistência e infraestrutura
📁 Microwave.Tests/          # Suite de testes (220 testes)
📁 WaveEngineCore/           # Web API + Interface web
```

## 🧪 Testes

```bash
dotnet test
# 220 testes passando ✅
```

## 🛠️ Stack

**Backend**: ASP.NET Core 9.0, JWT Authentication  
**Frontend**: Razor Pages, JavaScript  
**Arquitetura**: Clean Architecture/DDD  
**Testes**: xUnit  
**Persistência**: JSON + Session Storage

---

**Status**: 🚀 Nível 4 Completo | 🧪 220 testes | 🔐 JWT Auth | 📊 20 endpoints API
