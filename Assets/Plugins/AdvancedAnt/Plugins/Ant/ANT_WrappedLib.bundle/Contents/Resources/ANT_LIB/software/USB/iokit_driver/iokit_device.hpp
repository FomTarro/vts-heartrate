/*
This software is subject to the license described in the License.txt file 
included with this software distribution. You may not use this file except in compliance 
with this license.

Copyright (c) Dynastream Innovations Inc. 2013
All rights reserved.
*/

#ifndef IOKIT_DEVICE_HPP
#define IOKIT_DEVICE_HPP

#include "types.h"
#include "iokit_types.hpp"


#include <IOKit/usb/IOUSBLib.h>



//////////////////////////////////////////////////////////////////////////////////
// Public Definitions
//////////////////////////////////////////////////////////////////////////////////


//////////////////////////////////////////////////////////////////////////////////
// Public Class Prototypes
//////////////////////////////////////////////////////////////////////////////////

//NOTE: Supports IOUSBFamily 1.8.2 and later; Mac OS X 10.0.4 and later.
class IOKitDevice
{

   public:
      IOKitDevice();
      IOKitDevice(const io_service_t& hService_);
      IOKitDevice(const IOKitDevice& device);
      virtual ~IOKitDevice();
      
      IOKitDevice& operator=(const IOKitDevice& clDevice_);

      static const int MAX_CONFIG = 8;

      //attributes//

      io_service_t hSavedService;
      USHORT usAddress;
      USHORT usBusNumber;
      USHORT usConfigurationNum;
      ULONG ulSerialNumber;
      char szProductDescription[256];
      char szSerialString[256];
      USHORT usVid;
      USHORT usPid;

      IOUSBDeviceDescriptor stDeviceDescriptor;
      char szSystemPath[21];
      
      ULONG ulLocation;  //we use the location as our GUID
      static usb_device_t** CreateDeviceInterface(const io_service_t& hService_);

   private:
   
      IOKitError::Enum CheckDeviceSanity();
      static ULONG GetSerialNumber(const io_service_t& hService_, UCHAR* pucSerialString_, ULONG ulSize_, UCHAR& ucSNIndex);
      static ULONG GetDeviceNumber(const io_service_t& hService_, CFStringRef hProperty_);
      static BOOL GetDeviceString(const io_service_t& hService_, CFStringRef hProperty_, UCHAR* pucBsdName_, ULONG ulSize_, BOOL bSearchChildren_ = FALSE);
      
      static IOKitError::Enum GetNewDescriptor(usb_device_t** device, IOUSBDeviceDescriptor& dev_descriptor);

      
};

#endif // !defined(IOKIT_DEVICE_HPP)

