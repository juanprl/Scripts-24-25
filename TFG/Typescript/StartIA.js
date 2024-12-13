// index.js
const { LMStudioClient } = require("@lmstudio/sdk");
const fs = require('fs');

async function main() {
  //Reset  
  const now = new Date();
  const hours = now.getHours(); 
  const minutes = now.getMinutes();
  const seconds = now.getSeconds();
 
  let fullText = `${hours};${minutes};${seconds} == `;
  fs.writeFileSync('C:/Users/Jp/Desktop/iaRespuesta.txt', fullText, 'utf8'); 
  fullText = ``;
  
  // Create a client to connect to LM Studio, then load a model
  const client = new LMStudioClient();

  const downloadedModels = await client.system.listDownloadedModels();
  const downloadedLLMs = downloadedModels.filter((model) => model.type === "llm");

  let model = null;  
  const loadedModels = await client.llm.listLoaded();

  if(loadedModels.length > 0)//Cargar modelo si hay  
  {
    model = await client.llm.get({}); 
  }
  else
  {
    // Load the first model
    model = await client.llm.load(downloadedLLMs[0].path); 
  }

  // Predict!  
  let data;
  try { 
      data = fs.readFileSync('C:/Users/Jp/Desktop/iaPeticion.txt', 'utf8');//Para ejecutar esto, haz Ctrl + p, luego escribe > y le das a Quokka Start
  } catch (err) {
      console.error('Error al leer el archivo:', err);
  }   

  const prediction = model.respond([
    { role: "system", content: "You are a helpful AI assistant." },
    { role: "user", content: data},
  ]);

  for await (const text of prediction) 
  {
    fullText += text;  // Acumula el texto en la variable fullText 
  }

  // Escribir el texto final en un archivo .txt 
  fs.appendFileSync('C:/Users/Jp/Desktop/iaRespuesta.txt', fullText, 'utf8');
  
  console.log('Terminado'); 
}

main();