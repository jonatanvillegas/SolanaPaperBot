# Solana Paper Bot - ASP.NET Core Worker Service

Bot demo de paper trading para SOLUSDT. Usa precios reales de Binance Spot, pero no ejecuta órdenes reales.

## Qué hace

- Lee velas reales desde Binance.
- Calcula RSI, EMA9, EMA21 y EMA99.
- Simula entradas LONG y SHORT.
- Simula Take Profit y Stop Loss.
- Guarda estado e historial en `/app/data/state.json`.
- Controla meta diaria, pérdida máxima y máximo de trades por día.
- Está listo para Docker y Coolify.

## Uso local con Docker

```bash
cp .env.example .env
docker compose up -d --build
docker logs -f solana-paper-bot-dotnet
```

## Variables principales

```env
Bot__Symbol=SOLUSDT
Bot__Timeframe=15m
Bot__InitialBalance=100
Bot__DailyProfitTarget=2
Bot__DailyMaxLoss=3
Bot__TakeProfitPercent=0.006
Bot__StopLossPercent=0.004
Bot__PositionSizePercent=0.15
Bot__MaxTradesPerDay=5
```

## Despliegue en Coolify

1. Subir este proyecto a GitHub.
2. Crear una app en Coolify usando Docker Compose.
3. Agregar las variables de entorno del archivo `.env.example`.
4. Crear volumen persistente para `/app/data`.
5. Revisar logs del contenedor.

## Importante

Este proyecto es educativo. No garantiza ganancias. Primero debe probarse en modo demo durante varios días o semanas antes de pensar en dinero real.
