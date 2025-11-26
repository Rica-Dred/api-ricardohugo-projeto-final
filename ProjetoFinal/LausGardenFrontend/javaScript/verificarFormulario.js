  const contactoForm = document.getElementById("contactoForm");

  if(contactoForm){
    contactoForm.addEventListener('submit', function(){
      const contactoContainer = document.querySelector('.contacto-container');

      contactoContainer.innerHTML = '<h2>Mensagem Enviada!</h2>';
    });
  }

  