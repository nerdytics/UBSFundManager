# UBS Developer Assessment - C# WPF (Prism) 'FundManager' Application

# Acceptance Criteria
| Criteria                                                                                                           | Status        |
| ------------------------------------------------------------------------------------------------------------------ |:-------------:|
| Should be able to add an 'Equity' or 'Bond' stock to the fund portfolio via a panel at the top of the screen.      | Implemented   |
| To add a stock, the stock type must have been selected, the price indicated and quantity purchased specified       | Implemented   |
| All stocks in a fund portfolio should be visible in a center panel. This panel should display the 'Stock type', 'Stock name (generated from Stock type and count(stock type)', 'Price', 'Quantity', 'Market Value - (Price * Quantity)', 'Transaction Cost - (Market Value * 0.5% if an Equity Stock or Market Value * 2% for Bond stock                                                               | Implemented   |
| Stock name should be highlighted 'Red' for any stocks whose market value is < 0 or transaction cost > Tolerance where Tolerance = 100000 when stock type is 'Bond' or Tolerance = 200000 when stock type is 'Equity'                                          | Implemented   |
| On the right a panel, displaying summary level information of the portfolio                                        | Implemented   |

# Brief Overview
Basically the application consists of the following components:
* Window Service that listens on an AMQP host (deployed to an AzureVM as a cloud service)

* Azure DocumentDB NoSQL database (with stored procedures / user defined functions) for fast
query processing. Also uses MapR functions to improve query performance.

* Prism MVVM driven WPF UI (with Unity IoC)

* Messaging Library that abstracts AMQP connectivity from the WPF UI. Also enables easy publishing and receiving of messages from the AMQP host.

* In the 'AddFund' panel at the top, the button only gets enabled when all required information has been provided.
* In the 'FundSummary' panel to the right there is a miniature chart that visualises the information in the summary grid

# Screenshots - FundManager UI
https://cloud.githubusercontent.com/assets/26350963/23836207/9ffee326-076c-11e7-998e-d42f0563cc16.PNG
https://cloud.githubusercontent.com/assets/26350963/23836197/9fcd316e-076c-11e7-9d2f-4bb55f4512a3.PNG

# Screenshots - AMQP (RabbitMQ) ManagementUI
https://cloud.githubusercontent.com/assets/26350963/23836205/9fea73be-076c-11e7-9d94-ad75278be63c.PNG
https://cloud.githubusercontent.com/assets/26350963/23836204/9fe9e49e-076c-11e7-865b-707279ae42aa.PNG

# Screenshots - Azure IaaS [Host VM + Cloud Storage + Cloud Service]
https://cloud.githubusercontent.com/assets/26350963/23836199/9fd0c52c-076c-11e7-9972-5e9a156cc360.PNG
https://cloud.githubusercontent.com/assets/26350963/23836201/9fd1ecd6-076c-11e7-96b7-745d33a0f7b6.PNG
https://cloud.githubusercontent.com/assets/26350963/23836203/9fe910fa-076c-11e7-9f64-bcd1238caf0f.PNG
https://cloud.githubusercontent.com/assets/26350963/23836202/9fe5e3d0-076c-11e7-99a1-3149f8c589b2.PNG
https://cloud.githubusercontent.com/assets/26350963/23836198/9fd0b212-076c-11e7-8fea-cbee66386360.PNG
https://cloud.githubusercontent.com/assets/26350963/23836200/9fd0fe98-076c-11e7-907c-83d0c52bd863.PNG

