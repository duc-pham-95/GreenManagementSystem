
var slidePanelOpen; // flag set if panel is open or closed
var map;
var selectedPolygon = null;
var drawingManager;
var selectedMarker = null;
var recentlyAddedMarker = null;
var allTreeVisible = true;
var isEditingMarker = false;
var markerClusterer = null;
var currentUserID = null;
var drawnPolygon = null;


var disablePolygonColor = '#586881';
var selectedPolygonColor = '#33cccc';
var hoverPolygonColor = '#0099ff';

//listener variables
var mapClickListener = null;
var currentMarkerCompleteListener = null;
var mapZoomChangedListener = null;
var mapZoomChangedListener_NoHiddingTrees = null;


//variables for transition function
var numDeltas = 50;
var delay = 5;
var index = 0;
var deltaLat;
var deltaLng;


//constant
var EIU_LOCATION = { lat: 11.052991, lng: 106.666169 };
var animationTime = 300;
var minZoom = 17;



var polygons = [];

var trees = [];

var pendingTrees = [];

var treeTypes = new Map();

var treeTypesMap = new Map();

var managers = new Map()

var employees = new Map();

var users = new Map();

var markers = [];

var infoWindow;

var polygonData = [];

var treeIconSizes = [20, 30, 40, 50, 60, 70, 80];


//class declaration
function Plant(id, name, family, plantDate, manager, status, userId) {
    this.id = id;
    this.name = name;
    this.family = family;
    this.plantDate = plantDate;
    this.manager = manager;
    this.status = status;
    this.userId = userId;
}

function Tree(id, name, family, plantDate, manager, status, userId, location, height, bodyWidth, address, managedUnitId) {
    Plant.call(this, id, name, family, plantDate, manager, status, userId);

    this.location = location;//a marker object or lat,lng coordinates
    this.height = height;
    this.bodyWidth = bodyWidth;
    this.polygon = null;
    this.marker = null;
    this.address = address;
    this.managedUnitId = managedUnitId;
}

function Grass(id, name, family, plantDate, userId, manager, status, location, Area) {
    Plant.call(this, id, name, family, plantDate, manager, status, userId);

    this.location = location;//a polygon object or an array of coordinates
    this.Area = Area;//measured in squared metters
}

//Inheritance
Tree.prototype = Object.create(Plant.prototype);
Tree.prototype.constructor = Tree;

Grass.prototype = Object.create(Plant.prototype);
Grass.prototype.constructor = Grass;
//end class declaration




//onLoad function
$(function () {

    //info panel is not opened by default
    slidePanelOpen = false;
    $('#slide-panel').css('display', 'none');
    //close button is hidden by default
    $('#toggle').css('display', 'none');

    //close info panel button event
    $('#toggle').click(closePanel);

    //navigation button click event
    $('.sidebar-footer ul li:not(#toggle)').click(openPanel);


    //listener for clear button in the filter form    
    $('#clear-filter-btn').click(clickClearFilter_FilterForm);

    validateListenerOnSelect('seeding-region-select', 'Vui lòng chọn khu vực');
    validateListenerOnSelect('seeding-tree-type-select', 'Vui lòng chọn loại cây');
    validateListenerOnSelect('seeding-manager-select', 'Vui lòng chọn quản lý');
    validateListenerOnNumber('seeding-height-input', 'Vui lòng nhập chiều cao', 'Dữ liệu phải là kiểu số');
    validateListenerOnNumber('seeding-width-input', 'Vui lòng nhập chiều rộng', 'Dữ liệu phải là kiểu số');
    ValidateListenerForCoordinate_SeedingForm('seeding-lat-input', 'seeding-lat-input', 'seeding-lng-input', 'Vui lòng nhập kinh độ', 'Dữ liệu phải là kiểu số');
    ValidateListenerForCoordinate_SeedingForm('seeding-lng-input', 'seeding-lat-input', 'seeding-lng-input', 'Vui lòng nhập vĩ độ', 'Dữ liệu phải là kiểu số');

    //listener for clear button in the seeding form
    $('#seeding-clear-form-btn').click(clickClearSeedingForm);

    //////listener for save button in the seeding form
    $('#seeding-save-tree-btn').click(clickSaveTreeSeedingForm);

    // funtion for edit polygon
    $('#edit-polygon-btn').click(clickEditPolygon);

    $('#delete-polygon-btn').click(clickDeletePolygon);

    ////function for register region
    $('#register-polygon-draw-btn').click(clickRegisterRegionDrawingButton);
    $('#register-polygon-save-tree-btn').click(clickSave_RegisterRegionForm);
    $('#register-polygon-clear-form-btn').click(clickCancel_RegisterRegionForm);

    //window.addEventListener("resize", function () {
    //    if (window.innerWidth <= 1200) {
    //        $('#slide-panel').addClass('fixed-width');
    //        $('.sidebar-footer').addClass('relative-position');
    //    } else {
    //        $('.sidebar-footer').removeClass('relative-position');
    //        $('#slide-panel').removeClass('fixed-width');
    //    }
    //});

})


function initMap() {
    //set up map
    map = new google.maps.Map(document.getElementById('map'), {
        center: EIU_LOCATION,
        zoom: 18,
        streetViewControl: false,
        fullscreenControl: false,
        mapTypeControl: false,
        zoomControl: false,
        gestureHandling: 'greedy'
    });

    //setup info window
    infoWindow = new google.maps.InfoWindow({
        content: ''
    });

    google.maps.event.addListener(infoWindow, 'closeclick', () => {
        closeInfoWindow(infoWindow);
    });

    //set up drawing manager
    drawingManager = new google.maps.drawing.DrawingManager({
        drawingMode: null,
        drawingControl: false,
        drawingControlOptions: {
            position: google.maps.ControlPosition.TOP_CENTER,
            drawingModes: ['marker', 'polygon']
        },
        markerOptions: {
            icon: getIconURLByZoom(map.getZoom(), true)
        },
        polygonOptions: {
            fillColor: '#6c757d',
            fillOpacity: 0.4,
            strokeWeight: 0,
            strokeOpacity: 0,
            clickable: true,
            markers: [],
            treeMap: new Map(),
            name: '',
            manager: '',
            id: '',
            density_color: '#6c757d',
            ascendant: null,
            descendant: [],
            center: null,
            maxDistance: 0
        }
    });
    drawingManager.setMap(map);

    drawingManager.addListener('polygoncomplete', (polygon) => {

        //check for polygon overlapping
        var vertices = polygon.getPath();
        check = true;
        for (var i = 0; check && i < vertices.length; i++) {
            var point = vertices.getAt(i);
            for (var j = 0; j < polygons.length; j++) {
                var otherPolygon = polygons[j];
                if (distanceBetweenPoints(point, otherPolygon.center) <= otherPolygon.maxDistance
                    && isPointInPolygon(point, otherPolygon)) {
                    check = false;
                    break;
                }

                var vertices2 = otherPolygon.getPath();
                for (var k = 0; k < vertices2.length; k++) {
                    if (isPointInPolygon(vertices2.getAt(k), polygon)) {
                        check = false;
                        break;
                    }
                }

                if (!check) break;
            }
        }
        if (!check) {
            showSnackbar('Không vẽ phân vùng trên phân vùng đã có', 2);
            polygon.setMap(null);
            return;
        }

        drawingManager.setDrawingMode(null);//set drawing mode to default when drawing done

        drawnPolygon = polygon;
    });

    //Get data from database
    $.getJSON('/api/Area/child/id=UEIU')
        .done((polygonJSON) => {
            polygonJSON.forEach(region => {
                var array = [];
                region.polygon.coords.forEach(item => array.push({ lat: parseFloat(item.latitude), lng: parseFloat(item.longtitude) }));
                polygonData.push({ id: region.id, name: region.name, manager: region.managedUnitId, location: array, center: region.centerCoord, maxDistance: region.farthestDistance, parentAreaId: region.parentAreaId });
            });

            $.getJSON('/api/TreeName')
                .done(array => {
                    array.forEach(data => {
                        treeTypes.set(data.id, data.name);
                        treeTypesMap.set(data.name, 0);
                    });

                    //function for tree name management
                    $('#create-tree-name-btn').click(clickCreateTreeNameButton);

                    var html = '<option value="">Chọn loại cây</option>';
                    treeTypes.forEach((name, id) => html += `<option value="${id}">${name}</option>`);
                    //add tree types to select box in the seeding form
                    $('#seeding-tree-type-select').html(html);
                    //create filter tree button
                    html = '';
                    treeTypes.forEach((name, id) => html += `<button id="${id}" class="btn btn-primary tree-name-btn">${name}</button>`);
                    $('.list-button').html(html);

                    //get tree data
                    $.getJSON('/api/Tree/')
                        .done((treesJSON) => {
                            treesJSON.forEach(tree => {
                                trees.push(new Tree(tree.id, tree.name, tree.family, tree.regisDate, tree.employeeId, tree.status, tree.userId, [tree.coord.latitude, tree.coord.longtitude], tree.height, tree.width, tree.location, tree.managedUnitId));
                            });
                        })
                        .fail(function (jqxhr, textStatus, error) {
                            var err = textStatus + ", " + error;
                            console.log("Request Failed: " + err);
                        })
                        .always(() => {

                            //create polygons from polygonData
                            polygonData.forEach(addPolygons);
                            //add trees
                            trees.forEach(addTree);


                            ////cluster markers
                            //markerClusterer = new MarkerClusterer(map, markers, {
                            //    imagePath: 'https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m',
                            //    gridSize: 100,
                            //    minimumClusterSize: 1000
                            //});

                            //markerClusterer.clusters_.forEach(item=>item.clusterIcon_.hide())

                            //add a tree to the polygon that contains it
                            polygons.forEach(polygon => {
                                treeTypes.forEach((type, id) => {
                                    var count = polygon.markers.filter(item => item.title == type).length;
                                    if (count !== 0) {
                                        polygon.treeMap.set(type, count);
                                    }
                                    treeTypesMap.set(type, treeTypesMap.get(type) + count);
                                });
                                setColorOfAPolygon(polygon);
                            });

                            //add polygon names to select box into #region-select
                            $('#seeding-region-select').html('<option value="">Chọn phân vùng trên bản đồ</option>' + polygons.map(item => `<option value="${item.id}">${item.name}</option>`).join(''));

                            //listener for region select box
                            $('#seeding-region-select').change((evt) => {
                                var id = evt.target.value;
                                if (!id.length) {
                                    setMapAsDefault(false, true);
                                    $('#seeding-lat-input, #seeding-lng-input').prop('disabled', true);
                                    return;
                                }
                                $('#seeding-lat-input, #seeding-lng-input').prop('disabled', false);

                                clickPolygon(polygons.find(item => item.id === id));
                            })

                            //response to the map's zoom changes
                            enableMapZoomChangedListener();

                            //listener for map-click-postion button
                            $('#map-click-position-btn').click(clickGetPositionByClickingMap_SeedingForm);

                            //listener for map when click out of polygons or marker will reset as default
                            enableMapClickListener();

                            if ($('#userAuth').length) {
                                $.getJSON('/Account/Current/')
                                    .done((UserID) => {
                                        if (UserID) {
                                            currentUserID = UserID;
                                            $.getJSON('/api/UserPlants/id=' + UserID)
                                                .done((data) => {
                                                    data = data.plants; // []
                                                    data.forEach(id => {
                                                        marker = markers.find(x => x.tree.id === id);
                                                        if (marker) {
                                                            marker.set('icon', getCurrentUserTreeIconURLByZoom(map.getZoom()));
                                                            marker.isCurrentUser = true;
                                                        } else {
                                                            $.ajax({
                                                                type: 'DELETE',
                                                                url: `api/UserPlants/deleteTree/userId=${currentUserID}&&treeId=${id}`,
                                                                error: function (XMLHttpRequest, textStatus, errorThrown) {
                                                                    console.log('Không thể xóa reference cây id: ' + id);
                                                                },
                                                                success: function () {
                                                                    console.log('Đã xóa reference cây id: ' + id)
                                                                }
                                                            });
                                                        }
                                                    })
                                                })
                                                .fail(function (jqxhr, textStatus, error) {
                                                    var err = textStatus + ", " + error;
                                                    console.log("Request Failed: " + err);
                                                })
                                        }
                                    })
                                    .fail(function (jqxhr, textStatus, error) {
                                        var err = textStatus + ", " + error;
                                        console.log("Request Failed: " + err);
                                    })

                            }

                            $('#total-tree-name-input').val(treeTypes.size);
                            $('#total-tree-input').val(markers.length);

                            html = '';
                            treeTypes.forEach((name, id) => html += `
                                    <tr>
                                        <td>${name}</td>
                                        <td>${treeTypesMap.get(name)}</td>
                                        <td class="text-center">
                                            <button type="button" class="btn tree-name-edit-btn" data-toggle="modal" data-target="#delete-tree-name-modal" data-id="${id}"><i class="fa fa-pencil-alt"></i></button> | 
                                            <button type="button" class="btn tree-name-delete-btn" value="${id}"><i class="fa fa-trash-alt"></i></button>
                                        </td>
                                    </tr>`
                            );

                            $('#delete-tree-name-modal').on('show.bs.modal', function (event) {
                                var button = $(event.relatedTarget) // Button that triggered the modal
                                var id = button.data('id') // Extract info from data-* attributes
                                var modal = $(this)
                                modal.find('#current-tree-name-input').val(treeTypes.get(id));
                                modal.find('#id-input').val(id);
                            })

                            $('table#manage-tree-name-table tbody').html(html);

                            $('#save-edit-tree-name-btn').click(clickSaveEditTreeNameButton);
                            $('.tree-name-delete-btn').click(clickDeleteTreeNameButton);
                        });

                    //filter tree button click event
                    $('.list-button button').click((EventTarget) => {
                        var active = $(EventTarget.target).toggleClass('active').hasClass('active');

                        var num = filterMarkers(EventTarget.target.id, active);

                        EventTarget.currentTarget.innerText = treeTypes.get(EventTarget.currentTarget.id) + (active ? ' (' + num + ')' : '');
                    });




                })
                .fail(function (jqxhr, textStatus, error) {
                    var err = textStatus + ", " + error;
                    console.log("Request Failed: " + err);
                })

        })
        .fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Request Failed: " + err);
        })

    $.getJSON('/api/Employee')
        .done(array => {
            array.forEach(data => {
                employees.set(data.id, data);
            });

            $('#seeding-employee-select').html(() => {
                var text = '';
                for (var i of employees.values()) {
                    text += `<option value="${i.id}">${i.name}</option>`
                }
                return text;
            });
        })
        .fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Request Failed: " + err);
        })

    $.getJSON('/api/Unit')
        .done(array => {
            array.forEach(data => {
                managers.set(data.id, data);
            });

            $('#seeding-manager-select,#region-manager-select').html(() => {
                var text = '';
                for (var i of managers.values()) {
                    text += `<option value="${i.id}">${i.name}</option>`
                }
                return text;
            });
        })
        .fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Request Failed: " + err);
        })

    $.getJSON('/Account/all')
        .done(array => {
            array.forEach(data => {
                users.set(data.id, data);
            });

            var html = '';
            var list = Array.from(users.values());
            var func = (index) => {
                user = list[index];
                $.getJSON('/api/userPlants/id=' + user.id)
                    .done(data => {
                        html += `
                                    <tr>
                                        <td>${user.userName}</td>
                                        <td>${user.email}</td>
                                        <td>${user.phoneNumber ? user.phoneNumber : ""}</td>
                                        <td>${data.plants.length}</td>
                                        <td class="text-center">
                                            <button type="button" class="btn user-delete-btn" value="${user.id}"><i class="fa fa-trash-alt"></i></button>
                                        </td>
                                    </tr>`;
                        if (index == list.length - 1) {
                            $('#manage-user-table tbody').html(html);
                            $('.user-delete-btn').click(clickDeleteUserButton);
                            return;
                        }
                        func(index + 1);
                    })
                    .fail(function (jqxhr, textStatus, error) {
                        var err = textStatus + ", " + error;
                        console.log("Request Failed: " + err);
                    });
            }
            func(0);

        })
        .fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Request Failed: " + err);
        });

}

//begin panel functions
function openPanel() {

    if (!slidePanelOpen) {
        $('#slide-panel').animate({
            width: 'toggle'
        }, animationTime);
    }
    slidePanelOpen = true;

    infoWindow.close();

    setMapAsDefault(true, true);

    var id = this.id;

    $('.my-container').css('display', 'none')//hide all panels

    //open the corresponding one
    $(`#${id}-container`).css('display', 'block');

    $('#toggle').css('display', 'block');

    // all trees will be hidden when the filter panel first open
    if (id === 'filter') {
        //hide all trees
        setVisibleAlltrees(false);

        //reset filter buttons
        $('.list-button button').removeClass('active');

        //remove zoom changed listener
        disableMapZoomChangedListener();

        enableMapZoomChangedListener_NoHidingTrees();

        $('#clear-filter-btn').click();
    } else {
        //show all trees
        setVisibleAlltrees(true);

        enableMapZoomChangedListener();

        disableMapZoomChangedListener_NoHidingTrees();

        if (id === 'seeding') {
            map.panTo(EIU_LOCATION);
            map.setZoom(18);
            clearSeedingForm();
        } else if (id === 'partition') {
            fillRegionForm(null);
        }
        //else if (id === 'register-region') {
        //    clearRegisterRegionForm();
        //}
    }
}
function closePanel() {
    //switch info panel status
    slidePanelOpen = !slidePanelOpen;

    $("#slide-panel").animate({
        width: 'toggle'
    }, animationTime);

    $('#toggle').css('display', 'none');

    enableMapZoomChangedListener();
}
//end panel functions





///// begin click functions
function clickGetPositionByClickingMap_SeedingForm() {
    if (selectedPolygon === null) {
        alert('Vui lòng chọn phân vùng trước khi chọn vị trí đặt cây');
        return;
    }


    setMapAsDefault(true, false);

    infoWindow.close();

    showSnackbar("Chọn đến vị trí mới", 4);

    $('#seeding-region-select').prop('disabled', true);

    disableNavBarListener();

    disableMapZoomChangedListener();

    enableMapZoomChangedListener_NoHidingTrees();

    disableMapClickListener();

    markers.forEach(marker => {
        marker.setClickable(false);
        if (marker.polygon != selectedPolygon) {
            marker.setVisible(false);
        }
    });

    allTreeVisible = false;

    polygons.forEach(polygon => {
        polygon.set('clickable', false);

        if (polygon !== selectedPolygon) {
            polygon.set('fillColor', disablePolygonColor)
        }
    });

    if (recentlyAddedMarker !== null) {
        recentlyAddedMarker.setMap(null);
        recentlyAddedMarker = null;
        $('#seeding-lat-input').val('');
        $('#seeding-lng-input').val('');
    }

    drawingManager.setDrawingMode('marker');

    google.maps.event.removeListener(currentMarkerCompleteListener);

    //set listener when marker complete
    currentMarkerCompleteListener = drawingManager.addListener('markercomplete', (marker) => {
        marker.set('icon', getIconURLByZoom(map.getZoom(), true));
        if (!isPointInPolygon(marker.getPosition(), selectedPolygon)) {//user choose outside the polygon
            alert('Vui lòng chọn vị trí đặt cây nằm trong phân vùng đã chọn');
            marker.setMap(null);
            marker = null;
        } else {
            drawingManager.setDrawingMode(null);//set drawing mode to default when drawing done
            $('.has-danger').removeClass('has-danger');
            $('.error-message').hide();
            $('#seeding-lat-input').val(marker.getPosition().lat());
            $('#seeding-lng-input').val(marker.getPosition().lng());
            $('#seeding-lat-input-validation').removeClass('not-valid');
            $('#seeding-lng-input-validation').removeClass('not-valid');
            recentlyAddedMarker = marker;
            recentlyAddedMarker.set('draggable', true);
            recentlyAddedMarker.set('raiseOnDrag', true);
            validateDragTree_SeedingForm();
        }

    });
}

function clickClearFilter_FilterForm() {
    $('.list-button button').each((index, element) => {
        element.classList.remove('active');
        element.innerText = treeTypes.get(element.id);
    });

    setVisibleAlltrees(false);
}

function clickClearSeedingForm() {
    if (recentlyAddedMarker != null) {
        recentlyAddedMarker.setMap(null);
        recentlyAddedMarker = null;
    }


    enableMapZoomChangedListener();

    enableNavBarListener();

    disableMapZoomChangedListener_NoHidingTrees();

    enableMapClickListener();


    polygons.forEach(polygon => {
        setColorOfAPolygon(polygon);
        polygon.set('clickable', true);
    });

    markers.forEach(marker => {
        marker.setClickable(true);
        marker.setVisible(true);
    })


    drawingManager.setDrawingMode(null);

    setMapAsDefault(true, true);

    $('#seeding-region-select').prop('disabled', false);

    clearSeedingForm();
}

function clickSaveTreeSeedingForm() {
    if ($('#registerTree .error-message').parent().parent().hasClass('has-danger') || $('#registerTree .not-valid').length !== 0) {
        $('#registerTree .not-valid').html('Vui lòng nhập dữ liệu').show().parent().parent().addClass('has-danger')
        alert('Vui lòng nhập dữ liệu đúng theo yêu cầu');
        return;
    }

    //add tree directly to database
    if (confirm("Xác nhận?")) {
        //get selected Area id
        var sectionId = $('#seeding-region-select').find(":selected").val();
        $.getJSON("/api/Area/location/section=" + sectionId).done((Area) => {
            var object = {
                name: $('#seeding-tree-type-select').find(":selected").val(),
                family: '',
                height: $('#seeding-height-input').val(),
                width: $('#seeding-width-input').val(),
                status: $('#seeding-description-textArea').val(),
                regisDate: new Date(),
                coord: {
                    latitude: $('#seeding-lat-input').val(),
                    longtitude: $('#seeding-lng-input').val()
                },
                employeeId: $('#seeding-employee-select').find(":selected").val(),
                location: {
                    provinceId: Area.proId,
                    districtId: Area.disId,
                    wardId: Area.wardId,
                    unitId: Area.unitId,
                    sectionId: Area.secId
                },
                type: $('#seeding-tree-group-select').find(':selected').text(),
                managedUnitId: $('#seeding-manager-select').find(":selected").val(),
                userId: currentUserID
            };
            var jsonObject = JSON.stringify(object);
            $.ajax({
                type: 'POST',
                url: '/api/Tree',
                data: jsonObject,
                success: function (data) {

                    $.getJSON('api/UserPlants/id=' + currentUserID).done((userPlants) => {
                        var list = userPlants.plants;
                        list.push(data);
                        var userObject = {
                            userId: currentUserID,
                            plants: list
                        }
                        $.ajax({
                            type: 'PUT',
                            url: '/api/UserPlants',
                            data: JSON.stringify(userObject),
                            success: function (data) {
                                console.log(`Success api/UserPlants/id=' + ${currentUserID}`);
                            },
                            error: function (xmlhttprequest, textstatus, errorthrown) {
                                console.log(`Error : api/UserPlants/id=' + ${currentUserID}`);
                                console.log(xmlhttprequest.responseText);
                                console.log(textstatus);
                                console.log(errorthrown);
                                //showSnackbar('Đã xảy ra lỗi! Vui lòng liên hệ nhà phát triển', 2);
                            },
                            contentType: "application/json"
                        });
                    });

                    var newTree = new Tree(data, $('#seeding-tree-type-select').val(), '', Date.now(), $('#seeding-employee-select').val(), $('#seeding-description-textArea').val(), currentUserID, [$('#seeding-lat-input').val(), $('#seeding-lng-input').val()], $('#seeding-height-input').val(), $('#seeding-width-input').val());

                    var marker = new google.maps.Marker({
                        title: newTree.name,
                        position: new google.maps.LatLng(newTree.location[0], newTree.location[1]),
                        animation: null,
                        icon: getCurrentUserTreeIconURLByZoom(map.getZoom()),
                        map: map,
                        tree: newTree,
                        polygon: selectedPolygon,
                        click: null,
                        isCurrentUser: true
                    });

                    newTree.polygon = selectedPolygon;
                    newTree.marker = marker;
                    markers.push(marker);
                    trees.push(newTree);
                    selectedPolygon.markers.push(marker);
                    setColorOfAPolygon(selectedPolygon);

                    //update polygon treeMap
                    selectedPolygon.treeMap.set(newTree.name, (selectedPolygon.treeMap.get(newTree.name) || 0) + 1);

                    //clear the form after adding a tree
                    $('#seeding-clear-form-btn').click();

                    clickClearSeedingForm();

                    // Marker click listener
                    marker.click = marker.addListener('click', () => { clickMarker(marker) });

                    clickMarker(marker);

                    showSnackbar('Tạo thành công, tải lại trang', 2);
                    reloadPage(2);
                },
                error: function (xmlhttprequest, textstatus, errorthrown) {
                    console.log('xảy ra lỗi method post api/tree');
                    console.log(xmlhttprequest.responseText);
                    console.log(textstatus);
                    console.log(errorthrown);
                    showSnackbar('Đã xảy ra lỗi! Vui lòng liên hệ nhà phát triển', 2);
                    //reloadPage(2);
                },
                contentType: "application/json",
            });
        });


    }



}

//function deleteNull() {
//    var userPlants = $.getJSON('/api/userPlants')
//        .done(array => {
//            array.forEach(data => {
//                if (!users.has(data.userId)) {
//                    $.ajax({
//                        type: 'DELETE',
//                        url: 'api/UserPlants/' + data.userId,
//                        error: function (XMLHttpRequest, textStatus, errorThrown) {

//                            console.log('Delete  null user errors:');
//                            console.log(XMLHttpRequest.responseText);
//                            console.log(textStatus);
//                            console.log(errorThrown);
//                        },
//                        success: function () {
//                            console.log('delete success');
//                        }
//                    });

//                }
//            })

//        })
//}

function clickEditPolygon() {
    if (!selectedPolygon) return;
    var polygon = new google.maps.Polygon({
        paths: selectedPolygon.getPath().getArray()
    });

    $('#save-polygon-btn').toggle();
    $('#delete-polygon-btn').toggle();
    $('#cancel-polygon-btn').toggle();
    $('#edit-polygon-btn').toggle();

    $('#region-name-input').prop('readonly', false);
    $('#region-manager-input').prop('hidden', true);
    $('#region-manager-select').prop('hidden', false);


    //remove zoom changed listener
    disableMapZoomChangedListener();

    disableNavBarListener();

    disableMapClickListener();

    enableMapZoomChangedListener_NoHidingTrees();

    markers.forEach(marker => {
        if (marker.polygon !== selectedPolygon) {
            marker.setVisible(false);
        }

        marker.setClickable(false);
    });

    polygons.forEach(polygon => {
        polygon.set('clickable', false);
        polygon.set('fillColor', disablePolygonColor);
    });

    selectedPolygon.setEditable(true);
    selectedPolygon.set('fillColor', selectedPolygonColor);
    selectedPolygon.set('fillOpacity', 0.4);
    selectedPolygon.set('clickable', false);


    //let the map fitbounds the selectedPolygon
    var vertices = selectedPolygon.getPath();
    var bounds = new google.maps.LatLngBounds();
    for (var i = 0; i < vertices.length; i++) {
        var path = vertices.getAt(i);
        bounds.extend(new google.maps.LatLng(path.lat(), path.lng()));
    }
    map.fitBounds(bounds);

    $('#save-polygon-btn').off('click').click(clickSaveEditPolygonButton);
    $('#cancel-polygon-btn').off('click').click(() => { clickCancelEditPolygonButton(polygon) });
}

function clickDeletePolygon() {
    if (!selectedPolygon) return;

    if (confirm('Xác nhận xóa')) {
        $.ajax({
            type: 'DELETE',
            url: '/api/Area/' + selectedPolygon.id,
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                showSnackbar('Xảy ra lỗi, liên hệ team phát triển !.', 2);
                reloadPage(2);
                console.log('Delete polyogn errors:');
                console.log(XMLHttpRequest.responseText);
                console.log(textStatus);
                console.log(errorThrown);
            },
            success: function () {

                showSnackbar('Xóa thành công ! Tải lại trang', 2);

                reloadPage(2);
            }
        });


    }
}

function clickSaveEditPolygonButton() {

    var newCoords = [];
    var vertices = selectedPolygon.getPath();
    for (var i = 0; i < vertices.getLength(); i++) {
        var xy = vertices.getAt(i);
        newCoords.push({ latitude: xy.lat(), longtitude: xy.lng() });
    }

    var object = {
        id: selectedPolygon.id,
        name: $('#region-name-input').val(),
        managedUnitId: $('#region-manager-select').val(),//todo
        polygon: {
            coords: newCoords
        },
        centerCoord: selectedPolygon.center,
        farthestDistance: selectedPolygon.maxDistance,
        parentAreaId: 'UEIU',
        childAreaIds: [],
        leavesIds: []
    }
    var jsonObject = JSON.stringify(object);
    $.ajax({
        type: 'PUT',
        url: '/api/Area/',
        data: jsonObject,
        contentType: "application/json",
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showSnackbar('Xảy ra lỗi, liên hệ team phát triển !.', 2);
            reloadPage(2);
            console.log('change polygon info errors:');
            console.log(XMLHttpRequest.responseText);
            console.log(textStatus);
            console.log(errorThrown);

        },
        success: function () {
            //this function should only be called when the user changed the vertices of the polygon.
            var before = selectedPolygon.markers.length;
            selectedPolygon.markers.forEach(marker => {
                if (!isPointInPolygon(marker.getPosition(), selectedPolygon)) {
                    $.ajax({
                        type: 'DELETE',
                        url: '/api/Tree/' + marker.tree.id,
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            console.log('change polygon delete tree error:');
                            showSnackbar('Xảy ra lỗi khi xóa phân vùng, liên hệ team phát triển !.', 2);
                            console.log(XMLHttpRequest.responseText);
                            console.log(textStatus);
                            console.log(errorThrown);
                        },
                        success: function () {
                            console.log('change polygon delete tree success, tree id:' + marker.tree.id);
                        }
                    });
                    deleteMarker(marker);
                }
            })
            showSnackbar('Thay đổi thành công, số cây bị xóa đi: ' + (before - selectedPolygon.markers.length) + ', tải lại trang', 2);
            reloadPage(2);
        }
    });

}

function clickCancelEditPolygonButton(polygon) {
    $('#save-polygon-btn').toggle();
    $('#delete-polygon-btn').toggle();
    $('#cancel-polygon-btn').toggle();
    $('#edit-polygon-btn').toggle();

    //$('#region-manager-select').css('display', 'none');
    //$('#region-manager-input').css('display', 'block');

    $('#region-name-input').prop('readonly', true);
    $('#region-manager-input').prop('hidden', false);
    $('#region-manager-select').prop('hidden', true);

    //remove zoom changed listener
    enableMapZoomChangedListener();

    enableNavBarListener();

    enableMapClickListener();

    disableMapZoomChangedListener_NoHidingTrees();


    markers.forEach(marker => {
        //set clickable all trees
        marker.setClickable(true);
        marker.setVisible(true);
    });

    polygons.forEach(polygon => {
        setColorOfAPolygon(polygon);
        polygon.set('clickable', true);

    });

    selectedPolygon.set('fillColor', selectedPolygonColor);

    selectedPolygon.setMap(null);
    selectedPolygon.setPath(polygon.getPath());
    selectedPolygon.setMap(map);
    selectedPolygon.setEditable(false);

    fillRegionForm(selectedPolygon);
    polygon = null;
}

function clickPolygon(polygon) {

    if (selectedMarker) {
        setMapAsDefault(true, false);
        infoWindow.close();
    }

    if (selectedPolygon === polygon) {
        selectedPolygon = null;
        polygon.set('fillColor', polygon.density_color);
    } else {
        if (selectedPolygon !== null) {
            selectedPolygon.set('fillColor', selectedPolygon.density_color);
        }
        polygon.set('fillColor', selectedPolygonColor)
        selectedPolygon = polygon;
    }

    //response to the seeding form
    if (selectedPolygon !== null) {
        $('#seeding-region-select,#register-region-region-select').val(selectedPolygon.id);
        $('#seeding-region-select-validation').removeClass('not-valid').hide().parent().parent().removeClass('has-danger');
        $('#seeding-lat-input, #seeding-lng-input').prop('disabled', false);
        fillRegionForm(selectedPolygon);
    } else {
        $('#seeding-region-select,#register-region-region-select').val('');
        $('#seeding-region-select-validation').addClass('not-valid');
        $('#seeding-lat-input, #seeding-lng-input').prop('disabled', true);
        fillRegionForm(null);
    }

    console.log('selected polygon markers length: ' + polygon.markers.length);
}

function clickMarker(marker) {

    if (marker.tree === undefined) return;//this a grass and we do nothing cuz it's hard.

    //if (marker.polygon !== selectedPolygon) {
    //    clickPolygon(marker.polygon);
    //}

    var zoom = map.getZoom();
    //change icon of a tree when click on another tree or out of infoWindow
    if (selectedMarker === null) {
        if (marker.isCurrentUser) {
            marker.set('icon', getCurrentUserTreeIconURLByZoom(zoom));
        } else {
            marker.set('icon', getIconURLByZoom(zoom, true));
        }
    } else if (marker !== selectedMarker) {
        if (selectedMarker.isCurrentUser) {
            selectedMarker.set('icon', getCurrentUserTreeIconURLByZoom(zoom));
        } else {
            selectedMarker.set('icon', getIconURLByZoom(zoom, false));
        }
        if (marker.isCurrentUser) {
            marker.set('icon', getCurrentUserTreeIconURLByZoom(zoom));
        } else {
            marker.set('icon', getIconURLByZoom(zoom, true));
        }
        enableMarkerClickListener(selectedMarker);
        isEditingMarker = false;
    }


    selectedMarker = marker;

    disableMarkerClickListener(selectedMarker);

    console.log('clickmoreinfo function worked ' + marker.tree.name);

    var basicInfo = getBasicInfoString(marker);

    infoWindow.setContent(basicInfo);

    var detailInfo = getDetailInfoString(marker);

    google.maps.event.addListener(infoWindow, 'domready', () => {
        //hide the close button, user can only click cancel or save button
        $('#map > div > div > div:nth-child(1) > div:nth-child(3) > div > div:nth-child(4) > div:nth-child(2) > div > div > div > button').show();
        $('#delete-button').off('click').click(() => { clickDeleteMarkerInfoWindow(selectedMarker) });

        $('#edit-button').off('click').click(() => { clickEditTreeInfoWindow(detailInfo) });

    }, { once: true })

    infoWindow.open(map, marker);
}

function clickEditTreeInfoWindow(detailInfo) {
    if (slidePanelOpen)
        closePanel();

    isEditingMarker = true;

    disableNavBarListener();

    polygons.forEach(x => x.set('clickable', false));
    markers.forEach(x => x.set('clickable', false));

    map.set('minZoom', 17);

    infoWindow.close();

    infoWindow.setContent(detailInfo);

    infoWindow.open(map, selectedMarker);

    google.maps.event.addListener(infoWindow, 'domready', () => {
        //hide the close button, user can only click cancel or save button
        $('#map > div > div > div:nth-child(1) > div:nth-child(3) > div > div:nth-child(4) > div:nth-child(2) > div > div > div > button').hide();
    }, { once: true })

    //load employees options
    $('#infowindow-employee-select').html($('#seeding-employee-select').html());
    $('#infowindow-employee-select option[value=' + selectedMarker.tree.manager + ']').prop('selected', true);

    //enable editable info tree
    $('#infowindow-height-input').prop("readonly", false);
    $('#infowindow-width-input').prop("readonly", false);
    $('#infowindow-manager-select').prop("readonly", false);
    $('#infowindow-lat-input').prop("readonly", false);
    $('#infowindow-lng-input').prop("readonly", false);
    $('#infowindow-description-textArea').prop("readonly", false);

    validateListenerOnNumber('infowindow-height-input', 'Vui lòng nhập chiều cao', 'Dữ liệu phải là kiểu số');
    validateListenerOnNumber('infowindow-width-input', 'Vui lòng nhập chiều rộng', 'Dữ liệu phải là kiểu số');
    ValidateListenerForCoordinate_InfoWindow('infowindow-lat-input', 'infowindow-lat-input', 'infowindow-lng-input', 'Vui lòng nhập kinh độ', 'Dữ liệu phải là kiểu số');
    ValidateListenerForCoordinate_InfoWindow('infowindow-lng-input', 'infowindow-lat-input', 'infowindow-lng-input', 'Vui lòng nhập vĩ độ', 'Dữ liệu phải là kiểu số');

    $('#save-button').off('click').click(clickSaveTreeInfoWindow);

    $('#cancel-button').off('click').on('click', clickCancelButtonInfoWindow);

    $('#edit-map-click-position-btn').off('click').click(clickChoosingPositionOnMap_InfoWindow);
}

function clickSaveTreeInfoWindow() {

    if ($('#infowindowDetailInfo .error-message').parent().parent().hasClass('has-danger')) {
        alert('Vui lòng nhập dữ liệu đúng theo yêu cầu');
        return;
    }

    if (confirm('Xác nhận?')) {
        if ($('#infowindow-height-input').val().length > 10 || $('#infowindow-width-input').val().length > 10) {
            showSnackbar('Độ dài không hợp lệ!', 2);
            return;
        }
        var name = selectedMarker.tree.name;
        var height = parseFloat($('#infowindow-height-input').val());
        var width = parseFloat($('#infowindow-width-input').val());
        var status = $('#infowindow-description-textArea').val();
        var lat = $('#infowindow-lat-input').val();
        var lng = $('#infowindow-lng-input').val();
        var employeeId = $('#infowindow-employee-select').val();
        var object = {
            id: selectedMarker.tree.id,
            name: name,
            family: '',
            height: height,
            width: width,
            status: status,
            regisDate: selectedMarker.tree.plantDate,
            coord: {
                latitude: lat,
                longtitude: lng
            },
            employeeId: employeeId,
            location: selectedMarker.tree.address,
            type: "Thân Gỗ",
            managedUnitId: selectedMarker.tree.managedUnitId,
            userId: currentUserID
        };

        var jsonObject = JSON.stringify(object);
        $.ajax({
            type: 'PUT',
            url: '/api/Tree/',
            data: jsonObject,
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                showSnackbar('Xảy ra lỗi khi chỉnh sửa cây, liên hệ team phát triển !.', 2);
                reloadPage(2);
                console.log(XMLHttpRequest.responseText);
                console.log(textStatus);
                console.log(errorThrown);
            },
            success: function () {
                showSnackbar('Thay đổi thành công, tải lại trang', 2);
                reloadPage(2);
            },
            contentType: "application/json"
        });

    }

    //closeInfoWindow(infoWindow);

    //polygons.forEach(polygon => {
    //    setColorOfAPolygon(polygon);
    //    polygon.set('clickable', true);
    //});

    //markers.forEach(marker => {
    //    marker.setVisible(true);
    //    marker.set('clickable', true);
    //});

    //setMapAsDefault(true, true);

}

function clickCancelButtonInfoWindow() {

    isEditingMarker = false;

    map.set('minZoom', 0);

    enableNavBarListener();

    enableMapZoomChangedListener();

    disableMapZoomChangedListener_NoHidingTrees();

    selectedMarker.setPosition(new google.maps.LatLng(selectedMarker.tree.location[0], selectedMarker.tree.location[1]));

    closeInfoWindow(infoWindow);


    polygons.forEach(polygon => {
        setColorOfAPolygon(polygon);
        polygon.set('clickable', true);
    });


    markers.forEach(marker => {
        marker.setVisible(true);
        marker.set('clickable', true);
    });
}

function clickChoosingPositionOnMap_InfoWindow() {
    var polygon = selectedMarker.polygon;

    disableMapZoomChangedListener();

    enableMapZoomChangedListener_NoHidingTrees();

    infoWindow.close();

    markers.forEach(marker => {
        if (marker.polygon != polygon) {
            marker.setVisible(false);
        }
    });

    polygons.forEach(polygon => {
        polygon.set('fillColor', disablePolygonColor)
    });

    polygon.set('fillColor', selectedPolygonColor);

    selectedMarker.set('draggable', true);
    selectedMarker.set('raiseOnDrag', true);

    validateOnDragTree_InfoWindow();

}

function clickDeleteMarkerInfoWindow(marker) {
    console.log('delete tree button clicked ' + marker.tree.name);

    if (confirm('Xác nhận?')) {
        $.ajax({
            type: 'DELETE',
            url: '/api/Tree/' + marker.tree.id,
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log('Xảy ra lỗi, liên hệ team phát triển !.');
                console.log(XMLHttpRequest.responseText);
                console.logrt(textStatus);
                console.log(errorThrown);
                showSnackbar('Xảy ra lỗi, liên hệ team phát triển !.', 2);
                reloadPage(2);
            },
            success: function () {

                deleteMarker(marker);

                infoWindow.close();

                showSnackbar('Xóa cây thành công, tải lại trang', 2);
                reloadPage(2);
            }
        });
    }
}

function clickRegisterRegionDrawingButton() {
    if (drawingManager.drawingMode !== null) {
        drawingManager.setDrawingMode(null);
        if (drawnPolygon !== null)
            drawnPolygon.setMap(null);
        drawnPolygon = null;
    } else if (drawnPolygon !== null) {
        drawnPolygon.setMap(null);
        drawnPolygon = null;
    }
    drawingManager.setDrawingMode('polygon');

    disableNavBarListener();

    disableMapZoomChangedListener();

    enableMapZoomChangedListener_NoHidingTrees();

    disableMapClickListener();

    markers.forEach(marker => {
        marker.setClickable(false);
    });

    polygons.forEach(polygon => {
        polygon.set('clickable', false);
    });
}

function clickSave_RegisterRegionForm() {
    enableNavBarListener();
    enableMapZoomChangedListener();
    disableMapZoomChangedListener_NoHidingTrees();
    enableMapClickListener();
    drawingManager.setDrawingMode(null);
    var curId = $('#polygon-id-input').val();
    if (!(curId.startsWith('S') || curId.startsWith('s'))) {
        curId = 'S' + curId;
    }

    if (drawnPolygon === null || curId === '' || $('#polygon-name-input').val() === '') {
        showSnackbar('Vui lòng nhập đầy đủ thông tin', 2);
        return;
    }

    if (polygons.find(x => x.id.toLowerCase() === curId.toLowerCase()) !== undefined) {
        showSnackbar('ID đã tồn tại', 2);
        return;
    }

    var coords = [];
    var vertices = drawnPolygon.getPath();
    for (var i = 0; i < vertices.length; i++) {
        var path = vertices.getAt(i);
        coords.push({
            latitude: path.lat(),
            longtitude: path.lng()
        });
    }

    calculateCenterPoint_Polygon(drawnPolygon);

    var jsonObject = {
        id: curId,
        name: $('#polygon-name-input').val(),
        managedUnitId: "EIU",
        polygon: {
            coords: coords
        },
        centerCoord: drawnPolygon.center,
        farthestDistance: drawnPolygon.maxDistance,
        parentAreaId: "UEIU",
        childAreaIds: [],
        leavesIds: []
    }

    $.ajax({
        type: 'POST',
        url: '/api/Area',
        data: JSON.stringify(jsonObject),
        success: function (data) {
            showSnackbar('Thêm thành công! Đang tải lại trang', 2);
            reloadPage(2);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            console.log('xảy ra lỗi method post api/Area');
            console.log(xmlhttprequest.responseText);
            console.log(textstatus);
            console.log(errorthrown);
            showSnackbar('Đã xảy ra lỗi', 2);
            reloadPage(2);
        },
        contentType: "application/json",
    });
}

function clickCancel_RegisterRegionForm() {
    enableNavBarListener();
    enableMapZoomChangedListener();
    disableMapZoomChangedListener_NoHidingTrees();
    enableMapClickListener();
    drawingManager.setDrawingMode(null);
    if (drawnPolygon)
        drawnPolygon.setMap(null);

    drawnPolygon = null;

    $('#polygon-id-input').val('');
    $('#polygon-name-input').val('');

    markers.forEach(marker => {
        marker.setClickable(false);
    });

    polygons.forEach(polygon => {
        polygon.set('clickable', false);
    });
}

function clickCreateTreeNameButton() {
    var name = $('#manage-tree-name-input').val();
    if (name.length === 0) {
        alert('Vui lòng nhập tên cây');
        return;
    }

    if (Array.from(treeTypesMap.keys()).find(x => x.toLowerCase() === name.toLowerCase())) {
        alert(`${name} đã tồn tại`);
        return;
    }

    $.ajax({
        type: 'POST',
        url: '/api/TreeName',
        data: JSON.stringify({ name: name }),
        success: function (data) {
            showSnackbar('đăng ký thành công ! Tải lại trang', 2);
            reloadPage(2);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            console.log('xảy ra lỗi method post api/treename');
            console.log(xmlhttprequest.responseText);
            console.log(textstatus);
            console.log(errorthrown);
            showSnackbar('Đã xảy ra lỗi khi tạo loại cây Vui lòng liên hệ nhà phát triển', 2);
            reloadPage(2);
        },
        contentType: "application/json",
    });
}

function clickDeleteTreeNameButton(evt) {
    var id = evt.currentTarget.getAttribute("value");
    var name = treeTypes.get(id);
    if (confirm(`Xóa loại cây sẽ xóa toàn bộ cây với loại ${name}`)) {
        $.ajax({
            type: 'DELETE',
            url: `api/TreeName/${id}`,
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log('Không thể xóa loại cây: ' + id);
                showSnackbar('Không thể xóa loại cây: ' + name, 2);
            },
            success: function () {
                console.log('Đã xóa loại cây: ' + id);
                showSnackbar(`Đã xóa loại cây: ${name}! Tải lại trang`, 2);
                reloadPage(2);
            }
        });
    }
}

function clickSaveEditTreeNameButton() {
    var id = $('#id-input').val();
    var newName = $('#new-tree-name-input').val();
    if (newName.length === 0) {
        alert('Vui lòng nhập tên cây mới');
        return;
    }
    if (Array.from(treeTypes.values()).find(x => x === newName) !== undefined) {
        alert('Loại cây đã có trong hệ thống');
        return;
    }

    $.ajax({
        type: 'PUT',
        url: '/api/TreeName/',
        data: JSON.stringify({
            id: id,
            name: newName
        }),
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showSnackbar('Xảy ra lỗi khi chỉnh sửa cây, liên hệ team phát triển !.', 2);
            reloadPage(2);
            console.log(XMLHttpRequest.responseText);
            console.log(textStatus);
            console.log(errorThrown);
        },
        success: function () {
            showSnackbar('Thay đổi thành công, tải lại trang', 2);
            reloadPage(2);
        },
        contentType: "application/json"
    });

}


function clickDeleteUserButton(evt) {
    var id = evt.currentTarget.getAttribute("value");
    if (users.get(id).userName === 'Admin') {
        alert("Không thể xóa người quản lý");
        console.log('bị điên à?');
        return;
    }

    if (confirm(`Xóa nguời dùng sẽ xóa tất cả dữ liệu liên quan bao gồm cây đã trồng`)) {
        $.ajax({
            type: 'DELETE',
            url: `/Account/${id}`,
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log('Không thể xóa người dùng: ' + id);
                showSnackbar('Không thể xóa người dùng: ' + users.get(id).userName, 2);
            },
            success: function () {
                var count = 0;
                $.getJSON('/api/userPlants/id=' + id)
                    .done(data => {
                        data = data.plants; // []
                        data.forEach(treeId => {
                            $.ajax({
                                type: 'DELETE',
                                url: '/api/Tree/' + treeId,
                                error: function (XMLHttpRequest, textStatus, errorThrown) {
                                    console.log('Error delete user => delete tree id: ' + treeId);
                                    console.log(XMLHttpRequest.responseText);
                                    console.log(textStatus);
                                    console.log(errorThrown);
                                },
                                success: function () {
                                    console.log('Success delete user => delete tree id: ' + treeId);
                                    if (count++ === data.length - 1) {
                                        console.log('Đã xóa người dùng: ' + id);
                                        showSnackbar(`Đã xóa người dùng: ${users.get(id).userName}! Tải lại trang`, 2);
                                        reloadPage(2);
                                    }
                                }
                            });
                        });

                    })
                    .fail(function (jqxhr, textStatus, error) {
                        var err = textStatus + ", " + error;
                        console.log("Request Failed: " + err);
                    });


            }
        });
    }
}
///// end click functions


//// begin validation functions
function validateDragTree_SeedingForm() {

    var pos = recentlyAddedMarker.position;
    //set listener when marker complete
    recentlyAddedMarker.addListener('dragend', function () {
        if (!isPointInPolygon(recentlyAddedMarker.getPosition(), selectedPolygon)) {
            showSnackbar('Vui lòng đặt cây nằm trong vùng đã chọn', 2);

            //move the marker back
            recentlyAddedMarker.set('position', pos);

            //set clickable => user choose again
            recentlyAddedMarker.setClickable(true);
        } else {
            $('#seeding-lat-input').val(recentlyAddedMarker.getPosition().lat());
            $('#seeding-lng-input').val(recentlyAddedMarker.getPosition().lng());
        }
    });

}

function validateOnDragTree_InfoWindow() {
    //set listener when marker complete
    var pos = selectedMarker.getPosition();

    if (selectedMarker.dragendListener) {
        selectedMarker.dragendListener.remove();
    }
    selectedMarker.dragendListener = selectedMarker.addListener('dragend', function () {
        if (!isPointInPolygon(selectedMarker.getPosition(), selectedMarker.polygon)) {
            showSnackbar('Vui lòng đặt cây nằm trong vùng đã chọn', 2);


            //move the marker back
            selectedMarker.set('position', pos);


            //set clickable => user choose again
            selectedMarker.setClickable(true);
        } else {

            infoWindow.open(map, selectedMarker);

            $('#infowindow-lat-input').val(selectedMarker.getPosition().lat());
            $('#infowindow-lng-input').val(selectedMarker.getPosition().lng());
            $('#infowindow-lat').removeClass('has-danger');
            $('.error-message').css('display', 'none')
            $('#infowindow-lng').removeClass('has-danger');


            //hide the close button, user can only click cancel or save button
            $('#map > div > div > div:nth-child(1) > div:nth-child(3) > div > div:nth-child(4) > div:nth-child(2) > div > div > div > button').hide();

            selectedMarker.setClickable(false);

            selectedMarker.set('draggable', false);

            pos = selectedMarker.getPosition();
        }
    });

}

function validateListenerOnSelect(id, message) {
    var value;
    $(`#${id}`).focusout((evt) => {
        value = evt.currentTarget.value;
        if (value.length == 0) {
            $(`#${id}-validation`).show();
            $(`#${id}-validation`).html(message);
            $(`#${id}`).parent().parent().addClass($(`#${id}`).parent().parent().hasClass('has-danger') ? '' : 'has-danger');
            $(`#${id}`).parent().parent().addClass($(`#${id}`).parent().parent().hasClass('has-danger') ? '' : 'has-danger');
        } else {
            $(`#${id}-validation`).removeClass('not-valid');
            $(`#${id}-validation`).hide();
            $(`#${id}`).parent().parent().removeClass('has-danger');
        }
    })


}
function validateListenerOnNumber(id, message1, message2) {
    var value;
    $(`#${id}`).focusout((evt) => {
        value = evt.currentTarget.value;
        if (value.length == 0 || isNaN(value)) {
            $(`#${id}`).parent().parent().addClass($(`#${id}`).parent().parent().hasClass('has-danger') ? '' : 'has-danger');
            $(`#${id}-validation`).show();
            $(`#${id}-validation`).html(value.length == 0 ? message1 : message2);
        } else {
            $(`#${id}-validation`).removeClass('not-valid');
            $(`#${id}-validation`).hide();
            $(`#${id}`).parent().parent().removeClass('has-danger');
        }
    })

}
function ValidateListenerForCoordinate_SeedingForm(id, id1, id2, message1, message2) {
    $(`#${id}`).focusout((EventTarget) => {
        if (!selectedPolygon) {
            alert('Vui lòng chọn phân vùng trước');
            $(`#${id1}`).val('');
            $(`#${id2}`).val('');
            return;
        }
        value = EventTarget.currentTarget.value;

        if (value.length == 0 || isNaN(value)) {
            $(`#${id}`).parent().parent().removeClass('has-danger').addClass('has-danger');
            $(`#${id}-validation`).show();
            $(`#${id}-validation`).html(value.length == 0 ? message1 : message2);
        } else if (!$(`#${id1}`).parent().parent().hasClass('has-danger') && $(`#${id1}`).val().length != 0 && $(`#${id2}`).val().length != 0) {

            if (!isPointInPolygon(new google.maps.LatLng($(`#${id1}`).val(), $(`#${id2}`).val()), selectedPolygon)) {
                showSnackbar('Vui lòng đặt cây nằm trong phân vùng đã chọn', 2);
                $(`#${id1}`).val('');
                $(`#${id2}`).val('');
                $(`#${id1}-validation`).hide();
                $(`#${id2}-validation`).hide();
                if (recentlyAddedMarker) {
                    recentlyAddedMarker.setMap(null);
                    recentlyAddedMarker = null;
                }

            } else {
                $(`#${id}-validation`).removeClass('not-valid');

                if (!recentlyAddedMarker) {
                    recentlyAddedMarker = new google.maps.Marker({
                        position: new google.maps.LatLng($(`#${id1}`).val(), $(`#${id2}`).val()),
                        animation: null,
                        icon: getIconURLByZoom(map.getZoom(), true),
                        map: map
                    });
                } else {
                    transition(recentlyAddedMarker, new google.maps.LatLng($(`#${id1}`).val(), $(`#${id2}`).val()));
                }

            }
        } else {
            $(`#${id}-validation`).removeClass('not-valid');

            $(`#${id}-validation`).hide();
            $(`#${id}`).parent().parent().removeClass('has-danger');
        }
    })

}



function ValidateListenerForCoordinate_InfoWindow(id, id1, id2, message1, message2) {
    $(`#${id}`).focusout((EventTarget) => {
        value = EventTarget.currentTarget.value;
        if (value.length == 0 || isNaN(value)) {
            $(`#${id}`).parent().parent().removeClass('has-danger').addClass('has-danger');
            $(`#${id}-validation`).show();
            $(`#${id}-validation`).html(value.length == 0 ? message1 : message2);
        } else if (!$(`#${id1}`).parent().parent().hasClass('has-danger') && $(`#${id1}`).val().length != 0 && $(`#${id2}`).val().length != 0) {

            if (!isPointInPolygon(new google.maps.LatLng($(`#${id1}`).val(), $(`#${id2}`).val()), selectedMarker.polygon)) {
                showSnackbar('Vui lòng đặt cây nằm trong phân vùng đã chọn', 2);
                $(`#${id1}`).val(selectedMarker.getPosition().lat());
                $(`#${id2}`).val(selectedMarker.getPosition().lng());
                $(`#${id1}-validation`).hide();
                $(`#${id2}-validation`).hide();
            } else {
                $(`#${id}-validation`).removeClass('not-valid');

                transition(selectedMarker, new google.maps.LatLng($(`#${id1}`).val(), $(`#${id2}`).val()));
            }
        } else {
            $(`#${id}-validation`).removeClass('not-valid');

            $(`#${id}-validation`).hide();
            $(`#${id}`).parent().parent().removeClass('has-danger');
        }
    })

}

//// end validation functions



//// begin marker transition functions
function transition(marker, newPosition) {
    index = 0;
    deltaLat = (newPosition.lat() - marker.getPosition().lat()) / numDeltas;
    deltaLng = (newPosition.lng() - marker.getPosition().lng()) / numDeltas;
    moveMarker(marker);
}

function moveMarker(marker) {
    marker.set('position', new google.maps.LatLng((marker.getPosition().lat() + deltaLat), (marker.getPosition().lng() + deltaLng)));
    if (index != numDeltas) {
        index++;
        setTimeout(() => { moveMarker(marker) }, delay);
    }
}

//// end marker transition functions



// begin polygon properties setting functions
function setPropertiesNewlyCreatedPolygon(polygon) {

    polygon.addListener('click', () => { clickPolygon(polygon) });
    polygon.addListener('mouseover', polygonMouseOver);
    polygon.addListener('mouseout', polygonMouseOut);

    setEditableDraggble(polygon, false);
}

function polygonMouseOver(evt) {
    var polygon = this;
    if (polygon === selectedPolygon) return;
    polygon.setOptions({
        fillColor: hoverPolygonColor
    })
}

function polygonMouseOut(evt) {
    var polygon = this;
    if (polygon === selectedPolygon) return;
    polygon.setOptions({
        fillColor: polygon.density_color
    })
}

function setEditableDraggble(polygon, prop) {
    polygon.setDraggable(prop);
    polygon.setEditable(prop);
}

function setColorOfAPolygon(polygon) {
    var temp = polygon.markers.length;
    var value = [25, 50, 75, 100];
    var color = ['#82E0AA', '#52BE80', '#17A589', '#16A085'];
    for (var i = 0; i < value.length; i++) {
        if (temp <= value[i]) {
            polygon.setOptions({ fillColor: color[i], fillOpacity: 1, density_color: color[i] });
            break;
        }
    }
}

function togglePolygonColor(polygon) {
    if (polygon == null) return;
    polygon.set('fillColor', polygon.get('fillColor') === selectedPolygonColor ? polygon.density_color : selectedPolygonColor);
}
// end polygon properties setting functions


function showSnackbar(text, timeInSeconds) {
    // Get the snackbar DIV
    $('#snackbar').text(text).addClass('show');
    // After 3 seconds, remove the show class from DIV
    setTimeout(function () { $('#snackbar').removeClass('show') }, timeInSeconds * 1000);
}
//end snackbar functions


//// begin disable and enable listeners

//navbar
function disableNavBarListener() {
    $(`ul li`).off('click');
}

function enableNavBarListener() {
    $('ul li:not(#toggle)').off('click').click(openPanel);
    $('#toggle').off('click').click(closePanel);
}

//marker click
function disableMarkerClickListener(marker) {
    marker.click.remove();
    marker.click = null;
}

function enableMarkerClickListener(marker) {
    if (marker.click) return;
    marker.click = marker.addListener('click', () => { clickMarker(marker) });
}

// map click
function enableMapClickListener() {
    if (mapClickListener) return;

    mapClickListener = map.addListener('click', function (evt) {
        if (isEditingMarker) return;

        if (selectedMarker) {
            setMapAsDefault(true, false);
            infoWindow.close();
        }
        if (selectedPolygon) {
            clickPolygon(selectedPolygon);
        }
    });
}

function disableMapClickListener() {
    if (!mapClickListener) return;

    google.maps.event.removeListener(mapClickListener);
    mapClickListener = null;
}


//map zoom changed
function enableMapZoomChangedListener() {
    if (mapZoomChangedListener) return;

    if (!allTreeVisible && map.getZoom() >= minZoom) {
        setVisibleAlltrees(true);
    } else if (allTreeVisible && !(map.getZoom() >= minZoom)) {
        setVisibleAlltrees(false);
    }

    mapZoomChangedListener = google.maps.event.addListener(map, 'zoom_changed', function () {
        var zoom = map.getZoom();

        if (zoom >= minZoom) {
            var iconURL = getIconURLByZoom(zoom, false);
            var pendingIconURL = getCurrentUserTreeIconURLByZoom(zoom);
            if (!allTreeVisible) {
                markers.forEach(item => {
                    if (item.isCurrentUser) {
                        item.set('icon', pendingIconURL);
                    } else {
                        item.set('icon', iconURL);
                    }
                    item.setVisible(true);
                });

            } else {
                markers.forEach(item => {
                    if (item.isCurrentUser) {
                        item.set('icon', pendingIconURL);
                    } else {
                        item.set('icon', iconURL);
                    }
                });
            }
            allTreeVisible = true;

            if (selectedMarker) {
                if (selectedMarker.isCurrentUser) {
                    selectedMarker.set('icon', pendingIconURL);
                } else {
                    selectedMarker.set('icon', getIconURLByZoom(zoom, true));
                }
            }
            if (recentlyAddedMarker) {
                recentlyAddedMarker.set('icon', getIconURLByZoom(zoom, true));
            }
        } else if (allTreeVisible) {
            setVisibleAlltrees(false);
            infoWindow.close();
            if (selectedMarker) {
                enableMarkerClickListener(selectedMarker);
                selectedMarker = null;
            }
        }

        console.log('map zoom : ' + map.getZoom());
    });
}

function disableMapZoomChangedListener() {
    if (!mapZoomChangedListener) return;

    google.maps.event.removeListener(mapZoomChangedListener);
    mapZoomChangedListener = null;
}

function enableMapZoomChangedListener_NoHidingTrees() {
    if (mapZoomChangedListener_NoHiddingTrees) return;

    mapZoomChangedListener_NoHiddingTrees = google.maps.event.addListener(map, 'zoom_changed', function () {
        var zoom = map.getZoom();
        if (zoom >= minZoom) {
            var iconURL = getIconURLByZoom(zoom, false);
            var pendingIconURL = getCurrentUserTreeIconURLByZoom(zoom);

            markers.forEach(item => {
                if (item.isCurrentUser) {
                    item.set('icon', pendingIconURL);
                } else {
                    item.set('icon', iconURL);
                }
            });
            if (selectedMarker) {
                if (selectedMarker.isCurrentUser) {
                    selectedMarker.set('icon', pendingIconURL);
                } else {
                    selectedMarker.set('icon', getIconURLByZoom(zoom, true));
                }
            }
            if (recentlyAddedMarker) {
                recentlyAddedMarker.set('icon', getIconURLByZoom(zoom, true));
            }
        }
        //closeInfoWindow(infoWindow);
        console.log('map zoom : ' + map.getZoom());
    });
}

function disableMapZoomChangedListener_NoHidingTrees() {
    if (!mapZoomChangedListener_NoHiddingTrees) return;

    google.maps.event.removeListener(mapZoomChangedListener_NoHiddingTrees);
    mapZoomChangedListener_NoHiddingTrees = null;
}
//// end disable and enable listeners



function deleteMarker(marker) {
    var polygon = marker.polygon;
    //update polygon
    polygon.treeMap.set(marker.tree.name, polygon.treeMap.get(marker.tree.name) - 1);
    polygon.markers.splice(polygon.markers.findIndex(x => x === marker), 1);
    setColorOfAPolygon(polygon);

    //update trees array
    trees.splice(trees.findIndex(x => x === marker.tree), 1);
    //update markers array
    markers.splice(markers.findIndex(x => x === marker), 1);

    marker.setMap(null);
    marker = null;
}

function setVisibleAlltrees(visible) {
    allTreeVisible = visible;
    markers.forEach(item => item.setVisible(visible));
    pendingTrees.forEach(item => item.setVisible(visible));
}

function setClickableAllTrees(clickable) {
    markers.forEach(item => item.setClickable(clickable));
}

function setMapAsDefault(marker, polygon) {
    if (marker && selectedMarker !== null) {
        enableMarkerClickListener(selectedMarker);
        if (selectedMarker.isCurrentUser) {
            selectedMarker.set('icon', getCurrentUserTreeIconURLByZoom(map.getZoom()));
        } else {
            selectedMarker.set('icon', getIconURLByZoom(map.getZoom(), false));
        }
        selectedMarker = null;
    }

    if (polygon && selectedPolygon !== null) {
        selectedPolygon.set('fillColor', selectedPolygon.density_color);
        selectedPolygon = null;
    }

    isEditingMarker = false;
}


function getIconURLByZoom(zoom, isClicked) {
    return `google api/img/${isClicked ? 'clicked-' : ''}primary-tree/${isClicked ? 'clicked-' : ''}primary-tree-${treeIconSizes[zoom - minZoom]}.png`;
}

function getCurrentUserTreeIconURLByZoom(zoom) {
    return `google api/img/current user tree icon/current-user-tree-icon-${treeIconSizes[zoom - minZoom]}.png`;
}


function roundNumber(value, decimals) {
    return Number(Math.round(value + 'e' + decimals) + 'e-' + decimals);
}



function clearSeedingForm() {
    $('#seeding-region-select').val('');
    $('#seeding-tree-type-select').val('');
    $('#seeding-height-input').val('');
    $('#seeding-width-input').val('');
    $('#seeding-lat-input').val('');
    $('#seeding-lng-input').val('');
    $('#seeding-description-textArea').val('');
    $('.error-message').addClass('not-valid');
    $('#seeding-lat-input, #seeding-lng-input').prop('disabled', true);

    $('.has-danger').removeClass('has-danger');
    $('.error-message').hide();
}

//function clearRegisterRegionForm() {
//    $('#register-region-region-select').val('ancestor');
//    $('#register-region-region-name-input').val('');
//    $('#register-region-manager-input').val('');
//    $('#register-region-description-textArea').val('');

//    setMapAsDefault(false, true);
//}

function isPointInPolygon(point, polygon) {
    return google.maps.geometry.poly.containsLocation(point, polygon);
}


function filterMarkers(category, active) {
    return markers.filter(marker => {
        if (marker.tree.name == category) {
            marker.setVisible(active);
            return true;
        }
        return false;
    }).length;
}


function fillRegionForm(polygon) {
    if (polygon === null) {
        $('#coordinate-table tbody').html('');
        $('#calculateArea').val('');
        $('#tree-table tbody').html('');
        $('#region-name-input').val('');
        $('#region-manager-input').val('');
        $('#region-tree-quantity-input').val('');

        return;
    }
    $('#region-manager-input').val(polygon.manager);
    $('#region-name-input').val(polygon.name);
    $('#region-tree-quantity-input').val(polygon.markers.length);

    var vertices = polygon.getPath();
    //fill coordinate into table
    var coordinateTable = $('#coordinate-table tbody');
    coordinateTable.html('');
    // Iterate over the vertices.
    for (var i = 0; i < vertices.getLength(); i++) {
        var xy = vertices.getAt(i);
        coordinateTable.append(`<tr>
                                        <td title="${xy.lat()}">${roundNumber(xy.lat(), 5)}</td>
                                        <td title="${xy.lng()}">${roundNumber(xy.lng(), 5)}</td>
                                    </tr>`);
    }

    //show polygon Area
    $('#calculateArea').val(roundNumber(google.maps.geometry.spherical.computeArea(vertices), 4));

    var treeTable = $('#tree-table tbody');
    treeTable.html('');
    for (var [key, value] of polygon.treeMap) {
        if (value === 0) continue;
        treeTable.append(`<tr>
                                        <td>${key}</td>
                                        <td>${value}</td>
                                    </tr>`);
    }
}


function closeInfoWindow(infoWindow) {
    setMapAsDefault(true, true);
    infoWindow.close();
}


function addPolygons(data) {
    var polygon = new google.maps.Polygon({
        paths: data.location,
        fillColor: '#6c757d',
        fillOpacity: 0.4,
        strokeWeight: 0,
        strokeOpacity: 0,
        clickable: true,
        markers: [],
        treeMap: new Map(),
        name: data.name,
        manager: data.manager,
        id: data.id,
        density_color: '#6c757d',
        center: data.centerCoord,
        maxDistance: data.farthestDistance,
        parentAreaId: data.parentAreaId
    });
    if (polygon.center === undefined) {
        calculateCenterPoint_Polygon(polygon);
    }

    polygon.setMap(map);
    polygons.push(polygon);//add new polygon to array
    setPropertiesNewlyCreatedPolygon(polygon);
}

function addTree(tree) {
    if (!treeTypes.has(tree.name)) {
        $.ajax({
            type: 'DELETE',
            url: '/api/Tree/' + tree.id,
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log('Xảy ra lỗi, liên hệ team phát triển !.');
                console.log(XMLHttpRequest.responseText);
                console.logrt(textStatus);
                console.log(errorThrown);
                showSnackbar('Xảy ra lỗi, liên hệ team phát triển !.', 2);
                reloadPage(2);
            },
            success: function () {
                console.log('Loại cây đã xóa: ' + tree.name);
            }
        });
        return;
    }
    var pos = new google.maps.LatLng(tree.location[0], tree.location[1]);
    var marker = new google.maps.Marker({
        title: treeTypes.get(tree.name),//tree.name => id => lazy to change all the references
        position: pos,
        animation: null,
        icon: getIconURLByZoom(map.getZoom(), false),
        map: map,
        tree: tree,
        polygon: null,
        click: null,
        isCurrentUser: false
    });

    markers.push(marker);
    tree.marker = marker;
    //put tree to its container polygon and vice versa
    for (var i = 0; i < polygons.length; i++) {
        if (isPointInPolygon(pos, polygons[i])) {
            polygons[i].markers.push(marker);
            marker.polygon = polygons[i];
            marker.tree.polygon = polygons[i];
            break;
        }
    }

    // Marker click listener
    marker.click = marker.addListener('click', () => { clickMarker(marker) });
}

function getBasicInfoString(marker) {
    if ($('#adminAuth').length) {
        return `
        <form class="p-5.2">
            <div class="form-row ">
                <p><b>Tên cây trồng:</b> ${marker.title}</p>
            </div>
            <div class="form-row ">
                <p><b>Chiều cao cây:</b> ~${roundNumber(marker.tree.height, 1)}m</p>
            </div>
            <div class="form-row ">
                 <p><b>Đường kính thân:</b> ~${roundNumber(marker.tree.bodyWidth, 1)}cm</p>
            </div>
            <div class="form-row ">
                  <p><b>Ghi chú:</b> ${marker.tree.status != null ? marker.tree.status : ""}</p>
            </div>
            <div class="form-row ">
                  <p><b>Nhân Viên quản lý:</b> ${employees.get(marker.tree.manager) != null ? employees.get(marker.tree.manager).name : 'Không xác định'}</p>
            </div>
            <div class="form-row ">
                <p><b>Tên phân vùng:</b> ${marker.tree.polygon.name}</p>
            </div>
            <div class="form-row ">
                <p><b>Đề xuất trồng bởi:</b> ${users.get(marker.tree.userId) != null ? users.get(marker.tree.userId).userName : "Hệ Thống"}</p>
            </div>
            <div class="form-row mb-2">
                <div class="form-row">
                    <button style="position: absolute;left:35%"  id="edit-button"><i class="fa fa-pencil-alt"></i></button>
                    <button type="button" style="position: absolute;right:35% "id="delete-button"><i class="fa fa-trash-alt"></i></button>
                 </div>
            </div>
        </form>`
    } else {
        return `
        <form class="p-5.2">
            <div class="form-row ">
                <p><b>Tên cây trồng:</b> ${marker.title}</p>
            </div>
            <div class="form-row ">
                <p><b>Chiều cao cây:</b> ~${roundNumber(marker.tree.height, 1)}m</p>
            </div>
            <div class="form-row ">
                 <p><b>Đường kính thân:</b> ~${roundNumber(marker.tree.bodyWidth, 1)}cm</p>
            </div>
            <div class="form-row ">
                  <p><b>Ghi chú:</b> ${marker.tree.status != null ? marker.tree.status : ""}</p>
            </div>
            <div class="form-row ">
                  <p><b>Nhân Viên quản lý:</b> ${employees.get(marker.tree.manager) != null ? employees.get(marker.tree.manager).name : "Không xác định"}</p>
            </div>
            <div class="form-row ">
                <p><b>Tên phân vùng:</b> ${marker.tree.polygon.name}</p>
            </div>
            <div class="form-row ">
                <p><b>Đề xuất trồng bởi:</b> ${users.get(marker.tree.userId) != null ? users.get(marker.tree.userId).userName : 'Hệ Thống'}</p>
            </div>
        </form>`
    }
}

function getDetailInfoString(marker) {
    return `
        <form style="resize:both" class="p-5.2" id="infowindowDetailInfo">
            <div class="form-row mb-2 ">
                    <div class="col">
                        <label for="infowindow-region-input" class="bmd-label-static"> Phân vùng</label>
                        <input type="text" id="infowindow-region-input" class="form-control" value="${marker.tree.polygon.name}" disabled/>
                    </div>
                </div>
            <!-- tree group and tree type -->
            <div class="form-row mb-2">
                
                <div class="col">
                    <label for="infowindow-tree-type-input" class="bmd-label-static"> Loại cây</label>
                    <input type="text" id="infowindow-tree-type-input" class="form-control" value="${marker.title}" disabled/>
                    <input id="infowindow-tree-hidden-id" type="hidden" value="${marker.tree.id}"/>
                </div>
            </div>
            <!-- heght and width -->
            <div class="form-row mb-2">
                <div class="col">
                   <div>
                    <label for="infowindow-height-input" class="bmd-label-static">Chiều cao cây(m)</label>
                    <input type="text" name="infowindowHeightInput" id="infowindow-height-input" class="form-control" value="${roundNumber(marker.tree.height, 1)}" readonly>
                  <span id="infowindow-height-input-validation" class="error-message not-valid"></span>
                  </div>
                </div>
                <div class="col">
                    <div>
                    <label for="infowindow-width-input" class="bmd-label-static">Kích thước thân(cm)</label>
                    <input type="text" name="infowindowWidthInput" id="infowindow-width-input" class="form-control" value="${roundNumber(marker.tree.bodyWidth, 1)}" readonly>
                    <span id="infowindow-width-input-validation" class="error-message not-valid"></span>
                    </div>
                </div>
            </div>
            <!-- manage by -->
            
            <div class="form-row mb-2 ">
                <div class="col">
                   <label for="infowindow-employee-select" class="bmd-label-static"> Nhân viên quản lý</label>
                    <select id="infowindow-employee-select" class="browser-default custom-select mb-2">
                    </select>
                </div>
            </div>
            <!-- location -->
            <div class="form-row mb-2">
                <div class="col">
                        <label for="edit-map-click-position-btn" class="bmd-label-static">Địa điểm trồng :</label>
                        <button type="button" id="edit-map-click-position-btn" class="btn btn-info btn-block" style="background-color: gainsboro;font-weight: bold"" >Chọn vị trí trên bản đồ</button>
                </div>
            </div>
            <div class="form-row mb-2">
                        <div id="infowindow-lat" class="col col-sm-6">
                        <div>
                            <!-- latitude -->
                            <input type="text" name="infowindowLatInput" id="infowindow-lat-input" class="form-control" value="${marker.tree.location[0]}" readonly>
                            <span id="infowindow-lat-input-validation" class="error-message not-valid"></span>
                        </div>
</div>
                        <div id="infowindow-lng" class="col col-sm-6">
                        <div>
                            <!-- longtitude -->
                            <input type="text" name="infowindowLngInput" id="infowindow-lng-input" class="form-control" value="${marker.tree.location[1]}" readonly>
                            <span id="infowindow-lng-input-validation" class="error-message not-valid"></span>
                        </div>
</div>
            </div>
                    
            <!-- status -->
            <div class="form-row mb-2">
                <div class="col">
                    <label for="infowindow-description-textArea" class="bmd-label-static">Ghi chú:</label>
                    <textArea class="form-control rounded-1" id="infowindow-description-textArea" rows="3"  readonly>${marker.tree.status != null ? marker.tree.status : ""}</textArea>
                </div>
            </div>
            <!-- editbutton -->
            <div class="form-row mb-2">
                <div class="col-sm-6">
                   <button type="button" class="btn btn-info btn-block" style="background-color: gainsboro;font-weight: bold" id="save-button">Save</button>
                </div>
                <div class="col-sm-6">
                    <button type="button" class="btn btn-info btn-block" style="background-color: gainsboro;font-weight: bold" id="cancel-button">Cancel</button>
                </div>
            </div>  
        </form>`;
}


function distanceBetweenPoints(p1, p2) {
    if (!p1 || !p2) {
        return 0;
    }

    var R = 6371; // Radius of the Earth in km
    var dLat = (p2.lat() - p1.lat()) * Math.PI / 180;
    var dLon = (p2.lng() - p1.lng()) * Math.PI / 180;
    var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
        Math.cos(p1.lat() * Math.PI / 180) * Math.cos(p2.lat() * Math.PI / 180) *
        Math.sin(dLon / 2) * Math.sin(dLon / 2);
    var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    var d = R * c;
    return d;
}

function calculateCenterPoint_Polygon(polygon) {
    var vertices = polygon.getPath();
    var x1 = 90, y1 = 180, x2 = -90, y2 = -180;
    for (var i = 0; i < vertices.length; i++) {
        var path = vertices.getAt(i);
        x1 = Math.min(x1, path.lat());
        y1 = Math.min(y1, path.lng());
        x2 = Math.max(x2, path.lat());
        y2 = Math.max(y2, path.lng());
    }

    polygon.center = new google.maps.LatLng(x1 + ((x2 - x1) / 2), y1 + ((y2 - y1) / 2));
    polygon.maxDistance = Math.max(distanceBetweenPoints(polygon.center, new google.maps.LatLng(x1, y1)),
        distanceBetweenPoints(polygon.center, new google.maps.LatLng(x2, y2)));
}


function reloadPage(time) {
    setTimeout(() => { window.location.reload() }, time * 1000);
}