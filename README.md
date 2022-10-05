# Availability Monitor
## Table of contents
* [General info](#general-info)
* [Features](#features)
* [How it works](#how-it-works)
* [Screenshots](#screenshots) 
* [Technologies](#technologies)

## General info
The app was created to help monitor products availability and prices. It imports products from PrestaShop, then adds info about stock and prices from the supplier XML file and notifies when it changed.

## Features

* Importing products from your PrestaShop online store - no need to add them manually.
* Easily browse your products - sort and filter them as you need.
* Meaningful data charts - see the price and quantity for each product in past months.
* Notifications page - a quick insight into latest changes.


## How it works

* Import your products from PrestaShop by just entering your shop URL and API key in the configuration tab.
* Enter the URL of the supplier XML file with your products (products will be matched by the product SKU).
* Now you can update supplier info when you want and see all the changes.

## Screenshots

![ ](screenshots/screenshot_list_products.jpg)
![ ](screenshots/screenshot_list_products_2.jpg)
![ ](screenshots/screenshot_product_details.jpg)
![ ](screenshots/screenshot_product_details_2.jpg)

## Planned features
* Selecting few products at once from the list and make an action for each one (delete or update info).
* Filtering notifications.
* Ability to choose which data should be displayed in products' list.
* 

## Technologies

* .NET 6
* [Firebase](https://firebase.google.com/)
* Visual Studio