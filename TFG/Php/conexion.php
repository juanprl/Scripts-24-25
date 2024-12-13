<?php

$server = "localhost";
$user = "root";
$pass = "";
$db = "supermercado";

try {
    $pdo = new PDO("mysql:host=$server;dbname=$db", $user, $pass);
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    echo "Conectado";
} catch (PDOException $e) {
    die("Conexión Fallida: " . $e->getMessage());
}

?>
