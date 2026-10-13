/*
This software is subject to the license described in the License.txt file 
included with this software distribution. You may not use this file except in compliance 
with this license.

Copyright (c) Dynastream Innovations Inc. 2013
All rights reserved.
*/


#ifndef IOKIT_DEVICE_LIST_HPP
#define IOKIT_DEVICE_LIST_HPP

#include "types.h"

#include "iokit_types.hpp"
#include "iokit_device.hpp"


#include <vector>


//////////////////////////////////////////////////////////////////////////////////
// Public Definitions
//////////////////////////////////////////////////////////////////////////////////


//////////////////////////////////////////////////////////////////////////////////
// Public Class Prototypes
//////////////////////////////////////////////////////////////////////////////////

//!!Should we get devices in constructor so that we don't have to worry about freeing list?

class IOKitDeviceList
{
   public:
      static IOKitError::Enum GetDeviceList(std::vector<IOKitDevice*>& dev_list);
      static void FreeDeviceList(std::vector<IOKitDevice*>& dev_list);

      static io_service_t GetNextDevice(io_iterator_t deviceIterator);  //!!This is only public for the benefit of IOKitDeviceHandle but it would nice if we didn't have to do it

   private:
      static IOKitError::Enum MakeDeviceList(std::vector<IOKitDevice*>& dev_list);
	  
};


#endif // !defined(IOKIT_DEVICE_LIST_HPP)

