-- phpMyAdmin SQL Dump
-- version 3.5.7
-- http://www.phpmyadmin.net
--
-- Host: localhost:3306
-- Generation Time: May 07, 2013 at 04:06 PM
-- Server version: 5.5.29
-- PHP Version: 5.4.13

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Database: `whitefriday`
--

-- --------------------------------------------------------

--
-- Table structure for table `price_history`
--

CREATE TABLE IF NOT EXISTS `price_history` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `product_id` int(11) NOT NULL,
  `target_id` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `old_price` decimal(10,0) NOT NULL,
  `new_price` decimal(10,0) NOT NULL,
  `discount` decimal(10,0) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `product_id` (`product_id`,`target_id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=2 ;

--
-- Dumping data for table `price_history`
--

INSERT INTO `price_history` (`id`, `product_id`, `target_id`, `date`, `old_price`, `new_price`, `discount`) VALUES
(1, 1, 0, '2013-05-07 20:06:09', '1199', '999', '0');

-- --------------------------------------------------------

--
-- Table structure for table `products`
--

CREATE TABLE IF NOT EXISTS `products` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=2 ;

--
-- Dumping data for table `products`
--

INSERT INTO `products` (`id`, `name`) VALUES
(1, 'Smartphone Samsung Galaxy S III Mini Desbloqueado');

-- --------------------------------------------------------

--
-- Table structure for table `products_targets`
--

CREATE TABLE IF NOT EXISTS `products_targets` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `product_id` int(11) NOT NULL,
  `target_id` int(11) NOT NULL,
  `url` text NOT NULL,
  PRIMARY KEY (`id`),
  KEY `product_id` (`product_id`),
  KEY `target_id` (`target_id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=2 ;

--
-- Dumping data for table `products_targets`
--

INSERT INTO `products_targets` (`id`, `product_id`, `target_id`, `url`) VALUES
(1, 1, 0, 'http://www.submarino.com.br/produto/112402725/smartphone-samsung-galaxy-s-iii-mini-desbloqueado-branco-android-processador-dual-core-1ghz-tela-4-camera-5.0mp-3g-wi-fi-nfc-e-memoria-interna-8gb');

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
