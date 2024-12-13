<?php
set_time_limit(500); //LA IA ES LENA CON GANAS NECESITO ESTO


header("Content-Type: application/json");
include 'conexion.php';

$method = $_SERVER['REQUEST_METHOD'];
$input = json_decode(file_get_contents('php://input'), true);

// Verifica que la entrada JSON fue correctamente decodificada
if ($input === null) {
    die(json_encode(['error' => 'Entrada JSON inválida']));
}

// Validar el formato del JSON y los datos
if (!is_array($input)) {
    die(json_encode(['error' => 'El cuerpo de la solicitud debe ser un objeto JSON']));
}

// Validar valores individuales según el método
if (isset($input['nameIng']) && !is_string($input['nameIng'])) {
    die(json_encode(['error' => 'El valor de nameIng debe ser una cadena de texto']));
}
if (isset($input['cantidadGr']) && !is_numeric($input['cantidadGr'])) {
    die(json_encode(['error' => 'El valor de cantidadGr debe ser numérico']));
}

switch ($method) {
    case 'GET':
        handleGet($pdo);

        break;
    case 'POST':
        
        if (isset($input['nameIng'])) {
            handlePost($pdo, $input);
        }
        
        if (isset($input['prompt'])) {
            sendChatRequest($pdo, $input);
        }
        
        if (isset($input['urlIA'])) {
            test($pdo, $input);
        }

        break;
    case 'PUT':
        handlePut($pdo, $input);
        break;
    case 'DELETE':
        handleDelete($pdo, $input);
        break;
    case 'ASK':
            sendChatRequest($pdo, $input);
            break;
    default:
        echo json_encode(['message' => 'Invalid request method']);
        break;
}

function handleGet($pdo) {
    $sql = "SELECT * FROM ingrediente WHERE Cantidad_gr > 0";
    $stmt = $pdo->prepare($sql);
    $stmt->execute();
    $result = $stmt->fetchAll(PDO::FETCH_ASSOC);
    echo json_encode($result);
}

function handlePost($pdo, $input) {
    // Validar que los campos necesarios existan
    if (!isset($input['nameIng']) || !isset($input['typeIng'])) {
        echo json_encode(['error' => 'Faltan campos obligatorios (nameIng, typeIng)']);
        return;
    }

    $sql = "INSERT INTO ingrediente (Nombre_Ingrediente, Tipo_Ingrediente, Cantidad_gr) 
            VALUES (:nameIng, :typeIng, 1000)";
    $stmt = $pdo->prepare($sql);
    $stmt->execute(['nameIng' => $input['nameIng'], 'typeIng' => $input['typeIng']]);
    echo json_encode(['message' => 'Ingrediente añadido correctamente']);
}

function handlePut($pdo, $input) {
    // Validar que los campos necesarios existan
    if (!isset($input['nameIng']) || !isset($input['cantidadGr'])) {
        echo json_encode(['error' => 'Faltan campos obligatorios (nameIng, cantidadGr)']);
        return;
    }

    $sql = "UPDATE ingrediente SET Cantidad_gr = :cantidadGr WHERE Nombre_Ingrediente = :nameIng";
    $stmt = $pdo->prepare($sql);
    $stmt->execute(['nameIng' => $input['nameIng'], 'cantidadGr' => $input['cantidadGr']]);
    echo json_encode(['message' => 'Ingrediente actualizado correctamente']);
}

function handleDelete($pdo, $input) {
    // Validar que el campo necesario exista
    if (!isset($input['nameIng'])) {
        echo json_encode(['error' => 'Falta el campo obligatorio (nameIng)']);
        return;
    }

    $sql = "DELETE FROM ingrediente WHERE Nombre_Ingrediente = :nameIng";
    $stmt = $pdo->prepare($sql);
    $stmt->execute(['nameIng' => $input['nameIng']]);
    echo json_encode(['message' => 'Ingrediente borrado exitosamente']);
}

function sendChatRequest($pdo, $input) {
       
        // Validar que los campos necesarios existan 
        $requiredFields = ['urlIA', 'message', 'mode', 'userId'];
        foreach ($requiredFields as $field) {
            if (!isset($input[$field])) {
                echo json_encode(['error' => 'Falta el campo obligatorio (' . $field . ')']);
                return;
            }
        }
    
        $url = $input['urlIA'];
        $authorizationToken = 'JHW2CS8-81BM7TW-H331DVV-YS4MGYW';
        
        $data = [
            "message" => $input['message'],
            "mode" => $input['mode'],
            "userId" => $input['userId']
        ];
    
        // Inicializar cURL
        $curl = curl_init();
    
        curl_setopt_array($curl, [
            CURLOPT_URL => $url,
            CURLOPT_RETURNTRANSFER => true,
            CURLOPT_POST => true,
            CURLOPT_HTTPHEADER => [
                'Accept: application/json',
                'Authorization: Bearer ' . $authorizationToken,
                'Content-Type: application/json',
            ],
            CURLOPT_POSTFIELDS => json_encode($data),
        ]);
    
        // Ejecutar la solicitud
        $response = curl_exec($curl);
        $httpCode = curl_getinfo($curl, CURLINFO_HTTP_CODE);
    
        // Manejo de errores
        if ($response === false) {
            $error = curl_error($curl);
            curl_close($curl);
            echo json_encode([
                "success" => false,
                "error" => $error
            ]);
            return;
        }
    
        curl_close($curl);
    
        // Mostrar la respuesta y el código HTTP para depuración
        echo json_encode([
            "success" => true,
            "httpCode" => $httpCode,
            "response" => json_decode($response, true),
        ]);
        
        // Depuración adicional para verificar los datos y la URL
        echo "URL: " . $url . "\n";
        echo "Datos enviados: " . json_encode($data) . "\n";
        echo "Token de autorización: " . $authorizationToken . "\n";
}

function test($pdo, $input) {
        
    $url = $input['urlIA'];//

    $data = [
        'message' => $input['message'],
        'mode' => 'chat',
        'userId' => 1
    ];

    $headers = [
        'Accept: application/json',
        'Authorization: Bearer JHW2CS8-81BM7TW-H331DVV-YS4MGYW',
        'Content-Type: application/json'
    ];

    $ch = curl_init($url);
    
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
    curl_setopt($ch, CURLOPT_POST, true);
    curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode($data));

    $response = curl_exec($ch);
    
    if ($response === false) {
        echo 'Curl error: ' . curl_error($ch);
    } else {
        echo 'Response: ' . $response;
    }
    
    curl_close($ch);
}

?>
