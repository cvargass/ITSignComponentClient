window.loadDraggerFunctionality = async function (className) {
    const position = { x: 0, y: 0 };

    interact(className).draggable({
        listeners: {
            start(event) {
                //console.log(event.type, event.target);
            },
            move(event) {
                position.x += event.dx;
                position.y += event.dy;

                event.target.style.transform = `translate(${position.x}px, ${position.y}px)`;

                event.target.classList.add('dropped');
            },
            end(event) {
                //Positions calculated for PDF
                //x => (0, 555)
                //y => (0, 735)

                var nameDragItem = event.target.id;

                if (nameDragItem == "drag-item-rubric") {
                    SavePositionsRubric(event);
                } else {
                    SavePositionsGrafic(event);
                }
            }
        },
        modifiers: [
            interact.modifiers.restrictRect({
                restriction: document.querySelector(".drag-drop-zone"),
                endOnly: true,
            }),
        ],
    });
}

async function SavePositionsRubric(event) {
    // Value for enhancing experience and accuracy
    var valueAccurancyForX = 100;
    var valueAccurancyForY = 50;

    var dragger = document.getElementById("dragger");

    var coordinateX = Math.round((event.target.getBoundingClientRect().x - dragger.getBoundingClientRect().x) / 1.117);
    if (coordinateX >= 450)
        coordinateX = coordinateX - valueAccurancyForX;
    //console.log('Value in x signature: ' + coordinateX);

    var coordinateY = Math.round((event.clientY - (dragger.getBoundingClientRect().height + dragger.getBoundingClientRect().top)) * -1) - valueAccurancyForY;
    //console.log('Value in y signature: ' + coordinateY);

    window.localStorage.setItem("SignaturePosition", JSON.stringify({ CoordinateX: coordinateX, CoordinateY: coordinateY, NumberPage: 1 }));
}

async function SavePositionsGrafic(event) {
    // Value for enhancing experience and accuracy
    var valueAccurancyForX = 100;
    var valueAccurancyForY = 50;

    var dragger = document.getElementById("dragger");

    var coordinateX = Math.round((event.target.getBoundingClientRect().x - dragger.getBoundingClientRect().x) / 1.117);
    if (coordinateX >= 450)
        coordinateX = coordinateX - valueAccurancyForX;
    //console.log('Value in x grafic: ' + coordinateX);

    var coordinateY = Math.round((event.clientY - (dragger.getBoundingClientRect().height + dragger.getBoundingClientRect().top)) * -1) - valueAccurancyForY;
    //console.log('Value in y grafic: ' + coordinateY);

    window.localStorage.setItem("GraficPosition", JSON.stringify({ CoordinateX: coordinateX, CoordinateY: coordinateY, NumberPage: 1 }));
}

async function GetPositionSignature() {
    return window.localStorage.getItem("SignaturePosition");
}

async function GetPositionGrafic() {
    return window.localStorage.getItem("GraficPosition");
}

async function CleanPositions() {
    return window.localStorage.removeItem("SignaturePosition");
    return window.localStorage.removeItem("GraficPosition");
}

async function CleanSignaturePosition() {
    return window.localStorage.removeItem("SignaturePosition");
}

async function CleanGraficPosition() {
    return window.localStorage.removeItem("GraficPosition");
}