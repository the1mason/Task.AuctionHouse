# Онлайн Аукцион

За 3 дня я сделал простой API онлайн-аукциона. Ввиду небольшого срока, до дополнительных заданий в их подавляющем большинстве руки не дошли. Подробнее о том что сделано, а что нет - напишу в этом замечательном файлике.

# Большие решения

Структура проекта - результат моего освоения DDD  

ORM EfCore - Не самый чистый доменный код, но возможность сделать задание в премлемой степени за указанное время

Postgres - Современная, открытая, бесплатная

Базовый функционал vs flare - Приоретизация разработки основных сервисов и контроллеров в ущерб крутых фич.

## Система ролей  

Сделана базово через int-овый Enum. Простая как дверь, куда уж проще.

## Система денег  

У пользователях счет в виртуальных фантиках. Есть заблокированный и доступный балансы.  
Система платежей сделана просто: В `appsettings.json` конфиге указывается `Key`. На `/payments/withdraw` и `payments/deposit` приходят запросы с `id` аккаунта, кол-вом денег и этим самым `Key`. Если `Key` совпадает, даем добро на пополнение.

## Лоты и торги

Сделана базовая система лотов в более или менее полной мере с возможностью создать лот, сделать ставку, перебить ставку. Перебитую ставку можо отозвать (побеждающую нельзя!);
При ставке на лот, деньги резервируются на аккаунте пользователя.  

### Определяем победителей

Как мы определяем победителей по истечению времени? **Мы их не определяем.**  
Победитель после завершения торгов, должен сделать `claim` на указанный лот. Если это действительно победитель, деньги будут списаны с его аккаунта в пользу продавца, а лот будет считаться выигранным. Проигравшие же могут сделать `recall` своей ставки если ее перебили, тем самым сняв резерв с их средств.

Лот можно считать архивным, если он закрыт и не имеет ставок.

### Картинки

Для картинок планировал использовать локальное S3-совместимое хранилище вроде `less3` с sdk амазона. Руки не дошли.

### SignalR + RabbitMQ

Изначально у меня была красивая идея: Создать хаб в SignalR через который отправлять уведомления пользователям о том, что их ставку перебили, а уведомления об этом между всеми нодами распространять через rabbitmq, но к сожалению времени на это даже близко не хватило.

## Бонусы!

### `docker-compose up -d`

Ничто так не убивает, как потратить 2 часа на сетап локального окружения для проверки тестового задания. Хорошо, что этого делать не придется!